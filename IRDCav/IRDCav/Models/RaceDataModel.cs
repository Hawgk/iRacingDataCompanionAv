using System;
using static IRSDKSharper.IRacingSdkSessionInfo.DriverInfoModel;
using static IRSDKSharper.IRacingSdkSessionInfo.SessionInfoModel.SessionModel;

namespace IRDCav.Models
{
    public class RaceDataModel
    {
        public bool IsActive { get; set; }
        public bool IsMe { get; set; }
        public bool IsFastest { get; set; }
        public bool OnPitRoad { get; set; }
        public bool ConsiderForRelative { get; set; }

        public string Name { get; set; } = string.Empty;
        public string ClassStr { get; set; } = string.Empty;
        public string CarName { get; set; } = string.Empty;
        public string CarNumber { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public string ClassColor { get; set; } = string.Empty;

        public int Id { get; set; }
        public int Rating { get; set; }
        public int Class { get; set; }
        public int Position { get; set; }
        public int ClassPosition { get; set; }
        public int BestLapNum { get; set; }
        public int LapsCompleted { get; set; }

        public float LapDistPct { get; set; }
        public float LastLapTime { get; set; }
        public float BestLapTime { get; set; }
        public float FastestLapTime { get; set; }
        public float Interval { get; set; }

        public void SetFromLiveDataModel(LiveDataModel liveData)
        {
            Id = liveData.Id;
            Class = liveData.Class;
            LapDistPct = liveData.LapDistPct;
            OnPitRoad = liveData.OnPitRoad;
            Position = liveData.Position;
            ClassPosition = liveData.ClassPosition;
            Interval = liveData.Interval;
            ConsiderForRelative = liveData.ConsiderForRelative;
            LastLapTime = liveData.LastLapTime;
            BestLapTime = liveData.BestLapTime;
            BestLapNum = liveData.BestLapNum;
        }

        public void SetFromResultsModel(ResultsModel resultsModel)
        {
            Id = resultsModel.Id;
            IsFastest = resultsModel.IsFastest;
            Name = resultsModel.Name;
            ClassStr = resultsModel.Class;
            CarName = resultsModel.CarPath;
            License = resultsModel.License;
            ClassColor = resultsModel.ClassColor;
            FastestLapTime = resultsModel.FastestLapTime;
            LastLapTime = resultsModel.LastLapTime;
            Rating = resultsModel.Rating;
            ClassPosition = resultsModel.ClassPosition;
            LapsCompleted = resultsModel.LapsCompleted;
        }

        public void SetFromDriverModel(DriverModel driver)
        {
            string ratingString = ((float)driver.IRating / 1000).ToString("0.0") + "k";

            ClassColor = "#60" + driver.CarClassColor.Substring(2);
            Id = driver.CarIdx;
            Name = driver.UserName;
            ClassStr = driver.CarClassShortName;
            CarName = driver.CarPath;
            CarNumber = driver.CarNumber;
            License = driver.LicString.Split(" ")[0] + ratingString;
            Rating = driver.IRating;
        }

        public void SetFromPositionModel(PositionModel position)
        {
            Position = position.Position;
            ClassPosition = position.ClassPosition + 1;
            LapsCompleted = position.LapsComplete;
            FastestLapTime = position.FastestTime;
            LastLapTime = position.LastTime;

            if (Interval != 0 && !IsMe)
            {
                IsActive = true;
            }
        }
    }
}
