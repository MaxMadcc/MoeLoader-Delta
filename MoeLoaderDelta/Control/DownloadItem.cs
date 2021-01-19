﻿using System.ComponentModel;

namespace MoeLoaderDelta
{
    /// <summary>
    /// 下载状态
    /// </summary>
    public enum DLStatus { Success, Failed, Cancel, IsHave, DLing, Wait }

    /// <summary>
    /// 下载任务，用于界面绑定
    /// </summary>
    public class DownloadItem : INotifyPropertyChanged
    {
        private string size;
        private double progress;
        private DLStatus statusE;
        private double speed;
        private bool isSelected;

        public string FileName { get; set; }
        public string Host { get; set; }
        public string Author { get; set; }
        public string LocalName { get; set; }
        public string LocalFileName { get; set; }
        public int Id { get; set; }
        public bool NoVerify { get; set; }
        public string SearchWord { get; set; }
        public int SiteIntfcIndex { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// 大小
        /// </summary>
        public string Size
        {
            get => size;
            set
            {
                size = value;
                OnPropertyChanged("Size");
            }
        }

        /// <summary>
        /// 进度
        /// </summary>
        public double Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }

        /// <summary>
        /// 状态（图形表示）
        /// </summary>
        public string Status
        {
            get
            {
                switch (StatusE)
                {
                    case DLStatus.Wait:
                        return "/Images/wait.png";
                    case DLStatus.Success:
                        return "/Images/success.png";
                    case DLStatus.Cancel:
                        return "/Images/stop.png";
                    case DLStatus.IsHave:
                        return "/Images/goto.png";
                    case DLStatus.Failed:
                        return "/Images/failed.png";
                    case DLStatus.DLing:
                        return "/Images/dling.png";
                    default:
                        return "/Images/wait.png";
                }
            }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public DLStatus StatusE
        {
            get => statusE;
            set
            {
                statusE = value;
                OnPropertyChanged("Status");
                if (value != DLStatus.DLing) { SetSpeed(0.0); }
            }
        }

        public string Url { get; set; }

        public string Speed
        {
            get
            {
                if (statusE == DLStatus.DLing)
                {
                    return (speed > 1024.0
                           ? (speed / 1024.0).ToString("0.00 MB")
                           : speed.ToString("0.00 KB")) + "/s";
                }
                else { return string.Empty; };
            }
        }

        public void SetSpeed(double sp)
        {
            speed = sp;
            OnPropertyChanged("Speed");
        }

        /// <summary>
        /// 下载对象
        /// </summary>
        /// <param name="fileName">下载时文件名</param>
        /// <param name="url">下载链接</param>
        /// <param name="host">域名</param>
        /// <param name="author">上传者</param>
        /// <param name="localName">本地路径文件名</param>
        /// <param name="localfileName">本地文件名</param>
        /// <param name="id">作品ID</param>
        public DownloadItem(string fileName, string url, string host, string author, string localName, string localfileName, int id, bool? noVerify, string searchWord, int siteIntfcIndex)
        {
            FileName = fileName;
            Size = "N/A";
            Progress = 0;
            StatusE = DLStatus.Wait;
            Url = url;
            Host = host != null ? host.Trim() != "" ? host : "unDomain" : "unDomain";
            Author = author != null ? author.Trim() != "" ? author : "unAuthor" : "unAuthor";
            LocalName = localName;
            LocalFileName = localfileName;
            Id = id;
            NoVerify = noVerify != null ? (bool)noVerify : false;
            SearchWord = searchWord;
            SiteIntfcIndex = siteIntfcIndex;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
