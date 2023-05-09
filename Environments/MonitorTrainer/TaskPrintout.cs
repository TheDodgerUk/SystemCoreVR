using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonitorTrainer
{
    public class TaskPrintout
    {
        public void PrintOutData(List<MusicianRequestData> musicianRequestsCurrent, List<MusicianRequestData> musicianRequestsCompleted)
        {
            List<string> dataToWrite = CreateFileData(musicianRequestsCurrent, musicianRequestsCompleted);

            SaveFile(dataToWrite);
            musicianRequestsCurrent.Clear();
            musicianRequestsCompleted.Clear();
        }

        #region save file
        private List<string> CreateFileData(List<MusicianRequestData> musicianRequestsCurrent, List<MusicianRequestData> musicianRequestsCompleted)
        {
            List<string> dataToWrite = new List<string>();
            if (musicianRequestsCurrent.Count > 0 || musicianRequestsCompleted.Count > 0)
            {
                dataToWrite.Add("print out results");
                foreach (var item in musicianRequestsCurrent)
                {
                    dataToWrite.Add($"Still current item: {item.m_DescriptionForReport}, MaxTime: {item.m_LifespanSeconds}, finished in {item.m_CurrentTime}");
                }

                foreach (var item in musicianRequestsCompleted)
                {
                    dataToWrite.Add($"Completed item: {item.m_DescriptionForReport}, MaxTime: {item.m_LifespanSeconds}, finished in {item.m_CurrentTime}");
                }
            }

            foreach (var item in dataToWrite)
            {
                Debug.Log(item);
            }

            return dataToWrite;
        }

        private void SaveFile(List<string> dataToWrite)
        {
            string path = Crosstales.FB.FileBrowser.SaveFile("Save Request Data", "D:", "Save", "txt");

            if (false == string.IsNullOrEmpty(path))
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
                {
                    foreach (string line in dataToWrite)
                    {
                        file.WriteLine(line);
                    }
                }
            }
        }

        #endregion save file
    }
}
