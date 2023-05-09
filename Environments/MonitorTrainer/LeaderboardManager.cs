namespace MonitorTrainer
{
	using UnityEngine;
	using System.Collections.Generic;
	using Oculus.Platform.Models;
    using static MonitorTrainer.MonitorTrainerConsts;
    using System;

    // Coordinates updating leaderboard scores and polling for leaderboard updates.
    public class LeaderboardManager
	{
		public class LeaderBoardDebugData
		{
			public LeaderboardManager.LeaderboardDataEnum LeaderEnum;
			public int LeaderBoardScore;
		}

		public enum LeaderboardDataEnum
		{
			MOST_MATCHES_WON,
			HIGHEST_MATCH_SCORE,
			CURRENT_XP,
		}

		public LeaderboardManager()
        {
			Core.PhotonGenericRef.CollectLeaderBoardDataMessage<LeaderBoardDebugData>((data) =>
			{
                switch (data.LeaderEnum)
                {
                    case LeaderboardDataEnum.MOST_MATCHES_WON:
                        break;
                    case LeaderboardDataEnum.HIGHEST_MATCH_SCORE:
                        break;
                    case LeaderboardDataEnum.CURRENT_XP:
						var player = MonitorTrainer.MonitorTrainerRoot.Instance.PlayerChoiceDataRef;
						SubmitSongHighScore(player.SongDataRef.SongName, player.CurrentDifficulty, data.LeaderBoardScore);
						break;
                    default:
                        break;
                }
			});
		}

		// the top number of entries to query
		private const int TOP_N_COUNT = 10;

		// how often to poll the service for leaderboard updates
		private const float LEADERBOARD_POLL_FREQ = 30.0f;

		// the next time to check for leaderboard updates
		private float m_nextCheckTime;

		// cache to hold most-wins leaderboard entries as they come in
		private volatile SortedDictionary<int, LeaderboardEntry> m_mostWins;



		// cache to hold high-score leaderboard entries as they come in
		private volatile SortedDictionary<int, LeaderboardEntry> m_highScores;

		// cache to hold high-score leaderboard entries as they come in
		private volatile SortedDictionary<int, LeaderboardEntry> m_highScoreFriends;

		// whether we've found the local user's entry yet
		private bool m_foundLocalUserHighScore;


		public delegate void OnHighScoreLeaderboardUpdated(SortedDictionary<int, LeaderboardEntry> entries);
		// callback to deliver the high-scores leaderboard entries
		private OnHighScoreLeaderboardUpdated m_highScoreCallback;

		// callback to deliver the high-scores leaderboard entries
		private OnHighScoreLeaderboardUpdated m_highScoreFriendsCallback;

		public OnHighScoreLeaderboardUpdated HighScoreLeaderboardUpdatedCallback
		{
			set { m_highScoreCallback = value; }
		}

		public OnHighScoreLeaderboardUpdated HighScoreFriendsLeaderboardUpdatedCallback
		{
			set { m_highScoreFriendsCallback = value; }
		}


		public void LeaderboardsCheckForUpdatesGlobal()
		{
			CheckForUpdates(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName, MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty, MonitorTrainerConsts.LeaderboardEnum.Global);
		}

		public string CreateSongName(string songName, DifficultyModeEnum dificultyEnum) => $"{songName}_{dificultyEnum.ToString()}";

		private void CheckForUpdates(string songName, DifficultyModeEnum difficultyEnum, LeaderboardEnum leaderboardEnum)
		{

		    ////if(PlatformManager.CurrentState == PlatformManager.State.WAITING_TO_PRACTICE_OR_MATCHMAKE)
			{
				QueryHighScoreLeaderboard(songName, difficultyEnum, leaderboardEnum);
			}
		}


		private void QueryHighScoreLeaderboard(string songName , DifficultyModeEnum difficultyEnum, LeaderboardEnum leaderboardEnum)
		{
			// if a query is already in progress, don't start a new one.
			if (m_highScores != null)
				return;

			m_highScores = new SortedDictionary<int, LeaderboardEntry>();
			m_highScoreFriends = new SortedDictionary<int, LeaderboardEntry>();

			m_foundLocalUserHighScore = false;

			string combined = CreateSongName(songName, difficultyEnum);
			combined = combined.Replace(" ", "_");

			switch (leaderboardEnum)
            {
                case LeaderboardEnum.Global:
					Oculus.Platform.Leaderboards.GetEntries(combined, TOP_N_COUNT, Oculus.Platform.LeaderboardFilterType.None, Oculus.Platform.LeaderboardStartAt.Top).OnComplete((data) => HighestScoreGetEntriesCallback(data, combined));
					break;
                case LeaderboardEnum.Friend:
					Oculus.Platform.Leaderboards.GetEntries(combined, TOP_N_COUNT, Oculus.Platform.LeaderboardFilterType.Friends, Oculus.Platform.LeaderboardStartAt.Top).OnComplete((data) => HighestScoreFriendsGetEntriesCallback(data, combined));
					break;
                default:
                    break;
            }
		}


		void HighestScoreGetEntriesCallback(Oculus.Platform.Message<LeaderboardEntryList> msg, string songName)
		{
			if (!msg.IsError)
			{
				foreach (LeaderboardEntry entry in msg.Data)
				{
					m_highScores[entry.Rank] = entry;

					if (entry.User.ID == PlatformManager.MyID)
					{
						m_foundLocalUserHighScore = true;
					}
				}

				// results might be paged for large requests
				if (msg.Data.HasNextPage)
				{
					Oculus.Platform.Leaderboards.GetNextEntries(msg.Data).OnComplete((data) =>  HighestScoreGetEntriesCallback(data, songName));
					return;
				}

				// if local user not in the top, get their position specifically
				if (!m_foundLocalUserHighScore)
				{
					Oculus.Platform.Leaderboards.GetEntries(songName, 1, Oculus.Platform.LeaderboardFilterType.None,
						Oculus.Platform.LeaderboardStartAt.CenteredOnViewer).OnComplete((data) => HighestScoreGetEntriesCallback(data, songName));
					return;
				}
			}
			// else an error is returned if the local player isn't ranked - we can ignore that

			if (m_highScoreCallback != null)
			{
				m_highScoreCallback(m_highScores);
			}
			m_highScores = null;
		}

		void HighestScoreFriendsGetEntriesCallback(Oculus.Platform.Message<LeaderboardEntryList> msg, string songName)
		{
			if (!msg.IsError)
			{
				foreach (LeaderboardEntry entry in msg.Data)
				{
					m_highScoreFriends[entry.Rank] = entry;

					if (entry.User.ID == PlatformManager.MyID)
					{
						m_foundLocalUserHighScore = true;
					}
				}

				// results might be paged for large requests
				if (msg.Data.HasNextPage)
				{
					Oculus.Platform.Leaderboards.GetNextEntries(msg.Data).OnComplete((data) => HighestScoreGetEntriesCallback(data, songName));
					return;
				}

				// if local user not in the top, get their position specifically
				if (!m_foundLocalUserHighScore)
				{
					Oculus.Platform.Leaderboards.GetEntries(songName, 1, Oculus.Platform.LeaderboardFilterType.None,
						Oculus.Platform.LeaderboardStartAt.CenteredOnViewer).OnComplete((data) => HighestScoreFriendsGetEntriesCallback(data, songName));
					return;
				}
			}
			// else an error is returned if the local player isn't ranked - we can ignore that

			if (m_highScoreFriendsCallback != null)
			{
				m_highScoreFriendsCallback(m_highScoreFriends);
			}
			m_highScoreFriends = null;
		}




		public void SubmitSongHighScore(string songName, DifficultyModeEnum dificultyEnum, long score)
		{
			string songCombined = CreateSongName(songName, dificultyEnum);
			songCombined = songCombined.Replace(" ", "_");
			if (score > 0)
			{
				Oculus.Platform.Leaderboards.WriteEntry(songCombined, score).OnComplete(WriteEntry);
			}
		}

		private void WriteEntry(Oculus.Platform.Message<bool> message)
        {
			
			Debug.LogError($"Leaderboards.WriteEntry: {message.Data} message.IsError: {message.IsError}  messageError : {message.GetError().Message}");
		}

	}
}