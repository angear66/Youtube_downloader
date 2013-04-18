using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using YoutubeExtractor;
using System.Net;
using System.Diagnostics;
using BSD.FW.DBHelper;
using BSD.FW;

namespace YoutubeProxy
{
    public partial class _Default : System.Web.UI.Page
    {
        private static void DownloadVideo(IEnumerable<VideoInfo> videoInfos)
        {
            /*
             * Select the first .mp4 video with 360p resolution
             */
            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */

            //var client = new WebClient();
            //client.DownloadFile(video.DownloadUrl, Path.Combine("D:/Downloads", video.Title + video.VideoExtension));

            
            //var videoDownloader = new VideoDownloader(video, Path.Combine("D:/Downloads", video.Title + video.VideoExtension));

            //// Register the ProgressChanged event and print the current progress
            //videoDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);

            ///*
            // * Execute the video downloader.
            // * For GUI applications note, that this method runs synchronously.
            // */
            //videoDownloader.Execute();
        }

        public string GetConfig(string configName)
        {
            System.Configuration.AppSettingsReader rd = new System.Configuration.AppSettingsReader();
            return rd.GetValue(configName, typeof(string)).ToString();
        }
        public string FactoryName
        {
            get
            {
                AESSecurity aes = new AESSecurity();
                return  aes.Decrypt (this.GetConfig("DEFAULT_FACTORY"));
            }
        }
        public string ConnectionString
        {
            get
            {
                AESSecurity aes = new AESSecurity();
                return aes.Decrypt (this.GetConfig("DEFAULT_CONNECTION"));
            }
        }
        public WebProxy proxy()
        {
            if (this.GetConfig("USE_PROXY") == "N")
                return null;
            else
            {
                WebProxy proxy = new WebProxy(this.GetConfig ("PROXY_IP"),int.Parse ( this.GetConfig ("PROXY_PORT")));
                if(this.GetConfig ("USE_PROXY_AUTH")=="Y")
                    proxy.Credentials = new System.Net.NetworkCredential(this.GetConfig("PROXY_USER_NAME"), this.GetConfig("PROXY_PASSWORD"));
                return proxy;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!this.IsPostBack && Request.QueryString["v"] != null)
            {
                string id;
                id = Request.QueryString["v"].ToString();
                try
                {
                    string appCode = Request["AppCode"];
                    if (string.IsNullOrEmpty(appCode))
                        appCode = "APP_01";
                    DBHelper hlp = new DBHelper(new BSD.FW.DBHelper.DBHelperConnection(this.ConnectionString, this.FactoryName));
                    if (hlp.GetDataSet("select * from vod_youtube where v_id='" + id + "'", "youtube").Tables[0].Rows.Count == 0)
                    {

                        string link = "http://www.youtube.com/watch?v=";
                        link += id;

                        /*
                         * Get the available video formats.
                         * We'll work with them in the video and audio download examples.
                         */
                        WebProxy proxy = this.proxy();
                        IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link ,proxy,System.Text.Encoding.UTF8 );

                        VideoInfo video = videoInfos.First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);

                        WebClient client = new WebClient();
                        client.Proxy = proxy;
                        Response.Write(Server.MapPath("ffmpeg/ffmpeg.exe") + "<br>");
                        client.DownloadFile(video.DownloadUrl, Server.MapPath(id + ".mp4"));

                        System.Diagnostics.ProcessStartInfo p;
                        p = new System.Diagnostics.ProcessStartInfo(Server.MapPath("ffmpeg/ffmpeg.exe"), "-i \"" + Server.MapPath(id + ".mp4") + "\" -vcodec copy -acodec copy -vbsf h264_mp4toannexb \"" + Server.MapPath(id + ".ts") + "\"");
                        p.CreateNoWindow = true;
                        Process _p = new Process();
                        _p.StartInfo = p;
                        _p.Start();

                        _p.WaitForExit();


                        //Response.Clear();
                        //Response.TransmitFile(Server.MapPath(id + ".ts"));
                        _p.Close();
                        _p.Dispose();

//                        string sql = @"INSERT INTO VOD_YOUTUBE(V_ID,TITLE,PUBLISH_DATE,APP_CODE,THUMNAIL_URL,CREATED_DATE,CREATED_BY)VALUES
//                                ('" + id + "','" + video.Title + "',sysdate,'" + appCode + "','',sysdate,'VOD')";
//                        hlp.ExecuteNonQuery(sql);
                        string sql = "select * from VOD_YOUTUBE where 1<>1";
                        System.Data.DataTable dt = hlp.GetDataSet(sql, "VOD_YOUTUBE").Tables[0];
                        System.Data.DataRow dr = dt.NewRow();
                        dr["V_ID"] = id;
                        dr["TITLE"] = video.Title;
                        dr["PUBLISH_DATE"] = DateTime.Now ;
                        dr["THUMNAIL_URL"] = video.ThumnailUrl;
                        dr["APP_CODE"] = appCode;
                        dr["CREATED_DATE"] = DateTime.Now;
                        dr["CREATED_BY"] = "VOD";
                        
                        dt.Rows.Add(dr);
                        hlp.Update(dt.DataSet);

                        string fName = Server.MapPath(id + ".mp4");
                        //Delete mp4 file (temp file)
                        if (System.IO.File.Exists(fName))
                            System.IO.File.Delete(fName);

                        Response.Write("{ \"status\":200, \"description\":\"Success\"}");
                    }
                }
                catch (Exception ex) 
                {
                    string fName = Server.MapPath(id + ".mp4");
                    //Delete mp4 file
                    if (System.IO.File.Exists(fName))
                        System.IO.File.Delete(fName);
                    //Delete ts file
                    fName = Server.MapPath(id + ".ts");
                    if (System.IO.File.Exists(fName))
                        System.IO.File.Delete(fName); 

                    Response.Write(ex.Message);
                }
            }
        }
    }
}
