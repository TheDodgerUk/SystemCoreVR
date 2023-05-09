/**
 * Copyright (c) 2017 The Campfire Union Inc - All Rights Reserved.
 *
 * Licensed under the MIT license. See LICENSE file in the project root for
 * full license information.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;
using System;
using System.Collections;
using TMPro;

namespace VRKeys
{

    /// <summary>
    /// Keyboard input system for use with NewtonVR. To use, drop the VRKeys prefab
    /// into your scene and activate as needed. Listen for OnUpdate and OnSubmit events,
    /// and set the text via SetText(string).
    ///
    /// Input validation can be done during OnUpdate and OnSubmit events by calling
    /// ShowValidationMessage(msg) and HideValidationMessage(). The keyboard does not
    /// automatically hide OnSubmit, but rather you should call SetActive(false) when
    /// you have finished validating the submitted text.
    /// </summary>
    public class Keyboard : MonoBehaviour
    {
        public Vector3 positionRelativeToUser = new Vector3(0f, 1.35f, 2f);

        public KeyboardLayout keyboardLayout = KeyboardLayout.Qwerty;

        public string placeholderMessage = "Tap the keys to begin typing";


        [Space(15)]
        public Color displayTextColor = Color.black;

        public Color caretColor = Color.gray;

        [Space(15)]
        public GameObject keyPrefab;

        public Transform keysParent;

        public float keyWidth = 0.16f;

        public float keyHeight = 0.16f;

        [Space(15)]
        public string text = "";

        public GameObject keyboardWrapper;

        public ShiftKey shiftKey;

        public Key[] extraKeys;

        [Space(15)]
        public bool leftPressing = false;

        public bool rightPressing = false;

        public bool initialized = false;

        public bool disabled = true;

        public AudioClip m_KeyPress;
        private AudioSource m_AudioSource;

        private GameObject m_keyboardGameObject;

        [Serializable]
        public class KeyboardUpdateEvent : UnityEvent<string> { }

        [Serializable]
        public class KeyboardSubmitEvent : UnityEvent<string> { }

        [Space(15)]

        /// <summary>
        /// Listen for events whenever the text changes.
        /// </summary>
        public KeyboardUpdateEvent OnUpdate = new KeyboardUpdateEvent();

        /// <summary>
        /// Listen for events when Submit() is called.
        /// </summary>
        public KeyboardSubmitEvent OnSubmit = new KeyboardSubmitEvent();

        /// <summary>
        /// Listen for events when Cancel() is called.
        /// </summary>
        public UnityEvent OnCancel = new UnityEvent();


        private LetterKey[] keys;

        private bool shifted = false;

        private Layout layout;

        private Action<string> m_Action;


        public void SetStringCallback(Action<string> callback)
        {
            m_Action = callback;
        }

        public void CreateKeyboard(Action callback)
        {
            m_keyboardGameObject = this.transform.GetChild(0).gameObject;
            Core.Mono.StartCoroutine(DoSetLanguage(keyboardLayout, callback));
        }

        public void Show(bool state)
        {
            m_keyboardGameObject.SetActive(state);
        }

        /// <summary>
        /// Make sure mallets don't stay attached if VRKeys is disabled without
        /// calling Disable().
        /// </summary>
        private void OnDisable()
        {
            Disable();
        }

        /// <summary>
        /// Enable the keyboard.
        /// </summary>
        public void Enable()
        {
            if (!initialized)
            {
                // Make sure we're initialized first.
                Core.Mono.StartCoroutine(EnableWhenInitialized());
                return;
            }

            disabled = false;


            if (keysParent != null)
            {
                keysParent.gameObject.SetActive(true);
            }

            EnableInput();

        }

        private IEnumerator EnableWhenInitialized()
        {
            yield return new WaitUntil(() => initialized);

            Enable();
        }

        /// <summary>
        /// Disable the keyboard.
        /// </summary>
        public void Disable()
        {
            disabled = true;

            if (keysParent != null)
            {
                keysParent.gameObject.SetActive(false);
            }

        }

        /// <summary>
        /// Set the text value all at once.
        /// </summary>
        /// <param name="txt">New text value.</param>
        public void SetText(string txt)
        {
            text = txt;


            OnUpdate.Invoke(text);
        }

        /// <summary>
        /// Add a character to the input text.
        /// </summary>
        /// <param name="character">Character.</param>
        public void AddCharacter(string character)
        {
            text += character;

            OnUpdate.Invoke(text);

            if (shifted && character != "" && character != " ")
            {
                Core.Mono.StartCoroutine(DelayToggleShift());
            }
        }

        /// <summary>
        /// Toggle whether the characters are shifted (caps).
        /// </summary>
        public bool ToggleShift()
        {
            if (keys == null)
            {
                return false;
            }

            shifted = !shifted;

            foreach (LetterKey key in keys)
            {
                key.shifted = shifted;
            }

            shiftKey.Toggle(shifted);

            return shifted;
        }

        private IEnumerator DelayToggleShift()
        {
            yield return new WaitForSeconds(0.1f);

            ToggleShift();
        }

        /// <summary>
        /// Disable keyboard input.
        /// </summary>
        public void DisableInput()
        {
            leftPressing = false;
            rightPressing = false;

            if (keys != null)
            {
                foreach (LetterKey key in keys)
                {
                    if (key != null)
                    {
                        key.Disable();
                    }
                }
            }

            foreach (Key key in extraKeys)
            {
                key.Disable();
            }
        }

        /// <summary>
        /// Re-enable keyboard input.
        /// </summary>
        public void EnableInput()
        {
            leftPressing = false;
            rightPressing = false;


            if (keys != null)
            {
                foreach (LetterKey key in keys)
                {
                    if (key != null)
                    {
                        key.Enable();
                    }
                }
            }

            foreach (Key key in extraKeys)
            {
                key.Enable();
            }
        }

        /// <summary>
        /// Backspace one character.
        /// </summary>
        public void Backspace()
        {
            if (text.Length > 0)
            {
                text = text.Substring(0, text.Length - 1);
            }

            OnUpdate.Invoke(text);
        }

        /// <summary>
        /// Submit and close the keyboard.
        /// </summary>
        public void Submit()
        {
            OnSubmit.Invoke(text);
        }

        /// <summary>
        /// Cancel input and close the keyboard.
        /// </summary>
        public void Cancel()
        {
            OnCancel.Invoke();
            Disable();
        }


        private IEnumerator DoSetLanguage(KeyboardLayout lang, Action callback)
        {
            m_AudioSource = this.gameObject.ForceComponent<AudioSource>();
            m_AudioSource.clip = m_KeyPress;
            keyboardLayout = lang;
            layout = LayoutList.GetLayout(keyboardLayout);

            placeholderMessage = layout.placeholderMessage;

            yield return Core.Mono.StartCoroutine(SetupKeys());

            // Update extra keys
            foreach (Key key in extraKeys)
            {
                key.UpdateLayout(layout);
            }

            initialized = true;
            this.gameObject.SetActive(true);
            callback?.Invoke();
        }

        /// <summary>
        /// Set a custom placeholder message.
        /// </summary>
        /// <param name="msg">Message.</param>
        public void SetPlaceholderMessage(string msg)
        {
            Core.Mono.StartCoroutine(DoSetPlaceholderMessage(msg));
        }

        private IEnumerator DoSetPlaceholderMessage(string msg)
        {
            if (!initialized)
            {
                yield return new WaitUntil(() => initialized);
            }


            yield break;
        }


        /// <summary>
        /// Setup the keys.
        /// </summary>
        private IEnumerator SetupKeys()
        {
            bool activeState = true;

            // Hide everything before setting up the keys
            keysParent.gameObject.SetActive(false);

            // Remove previous keys
            if (keys != null)
            {
                foreach (Key key in keys)
                {
                    if (key != null)
                    {
                        GameObject.Destroy(key.gameObject);
                    }
                }
            }

            keys = new LetterKey[layout.TotalKeys()];
            int keyCount = 0;

            // Numbers row
            for (int i = 0; i < layout.row1Keys.Length; i++)
            {
                GameObject obj = (GameObject)GameObject.Instantiate(keyPrefab, keysParent);
                obj.transform.localPosition += (Vector3.right * ((keyWidth * i) - layout.row1Offset));

                LetterKey key = obj.GetComponent<LetterKey>();
                key.character = layout.row1Keys[i];
                key.shiftedChar = layout.row1Shift[i];
                key.shifted = false;
                key.Init(obj.transform.localPosition);

                obj.name = "Key: " + layout.row1Keys[i];
                obj.SetActive(true);

                keys[keyCount] = key;
                keyCount++;

                yield return null;
            }

            // QWERTY row
            for (int i = 0; i < layout.row2Keys.Length; i++)
            {
                GameObject obj = (GameObject)GameObject.Instantiate(keyPrefab, keysParent);
                obj.transform.localPosition += (Vector3.right * ((keyWidth * i) - layout.row2Offset));
                obj.transform.localPosition += (Vector3.back * keyHeight * 1);

                LetterKey key = obj.GetComponent<LetterKey>();
                key.character = layout.row2Keys[i];
                key.shiftedChar = layout.row2Shift[i];
                key.shifted = false;
                key.Init(obj.transform.localPosition);

                obj.name = "Key: " + layout.row2Keys[i];
                obj.SetActive(true);

                keys[keyCount] = key;
                keyCount++;

                yield return null;
            }

            // ASDF row
            for (int i = 0; i < layout.row3Keys.Length; i++)
            {
                GameObject obj = (GameObject)GameObject.Instantiate(keyPrefab, keysParent);
                obj.transform.localPosition += (Vector3.right * ((keyWidth * i) - layout.row3Offset));
                obj.transform.localPosition += (Vector3.back * keyHeight * 2);

                LetterKey key = obj.GetComponent<LetterKey>();
                key.character = layout.row3Keys[i];
                key.shiftedChar = layout.row3Shift[i];
                key.shifted = false;
                key.Init(obj.transform.localPosition);

                obj.name = "Key: " + layout.row3Keys[i];
                obj.SetActive(true);

                keys[keyCount] = key;
                keyCount++;

                yield return null;
            }

            // ZXCV row
            for (int i = 0; i < layout.row4Keys.Length; i++)
            {
                GameObject obj = (GameObject)GameObject.Instantiate(keyPrefab, keysParent);
                obj.transform.localPosition += (Vector3.right * ((keyWidth * i) - layout.row4Offset));
                obj.transform.localPosition += (Vector3.back * keyHeight * 3);

                LetterKey key = obj.GetComponent<LetterKey>();
                key.character = layout.row4Keys[i];
                key.shiftedChar = layout.row4Shift[i];
                key.shifted = false;
                key.Init(obj.transform.localPosition);

                obj.name = "Key: " + layout.row4Keys[i];
                obj.SetActive(true);

                keys[keyCount] = key;
                keyCount++;

                yield return null;
            }

            // Reset visibility of canvas and keyboard
            keysParent.gameObject.SetActive(activeState);
            foreach (var i in keys)
            {
                var ff = i.gameObject.ForceComponent<VrInteractionClickCallBack>();
                ff.AddCallback((amount) =>
                {
                    i.KeyPressed();
                    m_Action?.Invoke(i.keyboard.text);
                    m_AudioSource.Play();
                });
            }

            foreach (var i in extraKeys)
            {
                var ff = i.gameObject.ForceComponent<VrInteractionClickCallBack>();
                ff.AddCallback((amount) =>
                {
                    i.KeyPressed();
                    m_Action?.Invoke(i.keyboard.text);
                    m_AudioSource.Play();
                });
            }


        }

    }
}