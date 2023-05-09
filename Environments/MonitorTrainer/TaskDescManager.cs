using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonitorTrainer
{
    public class TaskDescManager : MonoBehaviour
    {
        public static TaskDescManager Instance;

        public List<Sprite> m_HeadsIcons = new List<Sprite>();

        [SerializeField] private Animator m_Animator;
        [SerializeField] private Image m_Head;
        [SerializeField] private Image m_HeadBackground;
        [SerializeField] private Image m_NameCorner;
        [SerializeField] private TextMeshProUGUI m_TaskDescription;
        [SerializeField] private TextMeshProUGUI m_TaskOwnerName;
        [SerializeField] private TextMeshProUGUI m_TaskOwnerTitle;

        private MusicianRequestData m_CurrentData;

        public bool m_IsVisible = false;

        public void Initialise(Dictionary<string, GameObject> content)
        {
            Instance = this;
            m_Animator = GetComponent<Animator>();

            m_Head = transform.SearchComponent<Image>("Head");
            m_HeadBackground = transform.SearchComponent<Image>("IconBackground");
            m_NameCorner = transform.SearchComponent<Image>("NameCorner");

            m_TaskDescription = transform.Find("Description/Description").GetComponent<TextMeshProUGUI>();
            m_TaskOwnerName = transform.Find("Name/Name").GetComponent<TextMeshProUGUI>();
            m_TaskOwnerTitle = transform.SearchComponent<TextMeshProUGUI>("Title");

            Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItemList(Core.Mono, (list) =>
            {
                foreach (var item in list)
                {
                    Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItemSprite(Core.Mono, item, (sprite) =>
                    {
                        if (sprite != null)
                        {
                            m_HeadsIcons.Add(sprite);
                        }
                    });
                }
            });
        }


        public void ClearAllTasks()
        {
            m_IsVisible = false;
        }
        public void Close(TaskDisplay selected)
        {
            if (selected.m_Data == m_CurrentData)
            {
                m_Animator.SetBool("Toggle", false);
                m_IsVisible = false;
                TaskBarManager.Instance.HighlightSlot(selected.m_CurrentSlot, false);
            }
        }

        public void Toggle(TaskDisplay selected, bool forceShow = false)
        {
            m_Animator.SetBool("TogglePrompt", selected.m_HurryPromptVisible);

            if (m_IsVisible == false || forceShow == true)
            {
                SetData(selected.m_Data);
                m_Animator.SetBool("Toggle", true);
                m_IsVisible = true;

                TaskBarManager.Instance.HighlightSlot(selected.m_CurrentSlot, true);
            }
            else
            {
                if (selected.m_Data != m_CurrentData)
                {
                    SwitchData(selected.m_Data);

                    TaskBarManager.Instance.HighlightSlot(selected.m_CurrentSlot, true);
                }
                else
                {
                    m_Animator.SetBool("Toggle", false);
                    m_IsVisible = false;

                    TaskBarManager.Instance.HighlightSlot(selected.m_CurrentSlot, false);
                }
            }
        }

        public void SwitchData(MusicianRequestData data)
        {
            m_Animator.SetInteger("RefreshStep", 1);

            this.WaitForFrames(2, () =>
            {
                SetData(data);
                m_Animator.SetInteger("RefreshStep", 2);
            });
        }

        public void TogglePrompt(TaskDisplay selected, bool state)
        {
            if (selected.m_Data == m_CurrentData && m_IsVisible)
            {
                m_Animator.SetBool("TogglePrompt", state);
            }
        }

        public void SetData(MusicianRequestData data)
        {
            m_CurrentData = data;

            m_TaskDescription.SetText(m_CurrentData.m_Description);
            var currentCharacterData = MonitorTrainerConsts.GetCharacterData(m_CurrentData.m_MainMusicianType);
            m_TaskOwnerName.SetText(currentCharacterData.Abbreviation);
            m_TaskOwnerTitle.SetText(currentCharacterData.CharacterName);

            var sprite = m_HeadsIcons.FindLast(e => e.name.ToLower() == currentCharacterData.CharacterName.ToLower());

            if(sprite == null)
            {
                //backup Radio image
                sprite = TaskDescManager.Instance.m_HeadsIcons.FindLast(e => e.name.ToLower() == "Daniel James".ToLower());
            }

            if (null != sprite)
            {
                Texture newIcon = sprite.texture;
                m_Head.material.SetTexture("_Image", newIcon); // old system 
                m_Head.sprite = sprite;                        // urp
            }
            else
            {
                Debug.LogError($"Cannot find image for {currentCharacterData.CharacterName}");
            }

            m_HeadBackground.material.SetColor("_EffectColour", currentCharacterData.Colour); // old system 
            m_HeadBackground.material.SetColor("_ImageColour", currentCharacterData.Colour); // old system 
            m_HeadBackground.color = currentCharacterData.Colour;                           // urp
            m_NameCorner.material.SetColor("_EffectColour", currentCharacterData.Colour); // old system 
            m_NameCorner.material.SetColor("_ImageColour", currentCharacterData.Colour); // old system 
            m_NameCorner.color = currentCharacterData.Colour;                           // urp
        }
    }
}