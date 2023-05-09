using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonitorTrainer
{
    public class FileManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup m_Group;
        [SerializeField] private Transform m_FoldersPanel;
        [SerializeField] private Transform m_SongsPanel;
        [SerializeField] private Transform m_SelectionPanel;

        [SerializeField] private Image m_SelectedSongCover;
        private List<Sprite> m_Covers = new List<Sprite>();
        private Dictionary<string, string[]> m_FoldersAndSongs = new Dictionary<string, string[]>();
        private const string EMPTY = "Empty";
        private GameObject m_SongPrefab;

        internal void Init(Dictionary<string, GameObject> content, Action onLoad)
        {
            GameObject folderPrefab = content["Folder"];
            m_SongPrefab = content["Song"];

            m_Covers = new List<Sprite>();
            var images = content["Song Covers"].GetComponentsInChildren<Image>();   

            foreach (Image item in images)
            {
                m_Covers.Add(item.sprite);
            }

            string date = DateTime.Now.ToString("dd");

            m_FoldersAndSongs.Add("P&J Live - 29/11/19", new string[] { EMPTY });
            m_FoldersAndSongs.Add("Symphony Hall - 09/12/19", new string[] { EMPTY });
            m_FoldersAndSongs.Add("London O2 - 24/12/19", new string[] { EMPTY });
            m_FoldersAndSongs.Add("MEN Arena - "+ date +"/02/20", new string[3] {
                                                            "Asylum of Decay",
                                                            "Hell's Hell",
                                                            "Right of Passage"});

            m_FoldersPanel = transform.Find("Left");
            m_SongsPanel = transform.Find("Mid");
            m_SelectionPanel = transform.Find("Right");

            m_Group = GetComponent<CanvasGroup>();

            //Spawn folders
            foreach (string item in m_FoldersAndSongs.Keys)
            {
                CreatePrefab(item, folderPrefab, m_FoldersPanel, OnFolderItemSelected);
            }

            transform.FindComponent<Button>("Right/BtnLoad").onClick.AddListener(() => onLoad());

            m_SelectedSongCover = transform.Find("Right/Cover").GetComponent<Image>();

            TogglePanel(m_SongsPanel, false);
            TogglePanel(m_SelectionPanel, false);
        }

        private void CreatePrefab(string content, GameObject prefab, Transform parent, Action<bool, string> onToggled)
        {
            GameObject item = Instantiate(prefab, parent);
            item.transform.FindComponent<TextMeshProUGUI>("Label").SetText(content);
            item.GetComponent<Toggle>().group = parent.GetComponent<ToggleGroup>();
            item.GetComponent<Toggle>().onValueChanged.AddListener((value) => onToggled(value, content));
            item.SetActive(true);
        }

        private void CreateEmpty(GameObject prefab, Transform parent)
        {
            //buffer to move it to the middle
            for (int i = 0; i < 5; i++)
            {
                GameObject item = Instantiate(prefab, parent);
                TextMeshProUGUI text = item.transform.FindComponent<TextMeshProUGUI>("Label");
                Image background = item.transform.FindComponent<Image>("Background");
                background.enabled = false;
                text.fontStyle = FontStyles.Italic;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.grey;

                if (i == 4) text.SetText(EMPTY);
                else text.SetText(string.Empty);

                item.GetComponent<Toggle>().interactable = false;
                item.SetActive(true);
            }
        }

        public void VisibleAndInteractive(bool state)
        {
            m_Group.VisibleAndInteractive(state);
        }

        private void OnFolderItemSelected(bool state, string selection)
        {
            TogglePanel(m_SongsPanel, state);

            if (state)
            {
                foreach (Transform item in m_SongsPanel)
                {
                    Destroy(item.gameObject);
                }
                
                string[] songs = m_FoldersAndSongs[selection];

                if (songs[0] == EMPTY)
                {
                    TogglePanel(m_SelectionPanel, false);

                    CreateEmpty(m_SongPrefab, m_SongsPanel);
                }
                else
                {
                    for (int i = 0; i < songs.Length; i++)
                    {
                        CreatePrefab(songs[i], m_SongPrefab, m_SongsPanel, OnSongItemSelected);
                    }
                }

            }
        }

        private void OnSongItemSelected(bool state, string selection)
        {
            TogglePanel(m_SelectionPanel, state);

            if (state)
            {
                //we also assume that the song has a coresponding cover with a matching sprite name
                m_SelectedSongCover.sprite = m_Covers.Find(e => e.name == selection);
            }
        }

        private void TogglePanel(Transform panel, bool state)
        {
            CanvasGroup canvas = panel.GetComponent<CanvasGroup>();
            canvas.VisibleAndInteractive(state);
        }
    }
}