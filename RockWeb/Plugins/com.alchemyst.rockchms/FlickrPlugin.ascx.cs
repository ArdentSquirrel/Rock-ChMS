using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using FlickrNet;


namespace com.alchemyst.rockchms
{
    [TextField("Flickr AuthToken", "The Flickr Auth Token", required: false, key: "FlickrAuthToken")]
    [TextField("Flickr AuthTokenSecret", "The Flickr Auth Token Secret", required: true, key: "FlickrAuthTokenSecret")]
    [TextField("Flickr UserId", "The Flickr UserId", required: true, key: "FlickrUserId")]
    [TextField("Flickr Username", "The Flickr Username", required: true, key: "FlickrUsername")]
    public partial class FlickrPlugin : Rock.Web.UI.RockBlock
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Check for return authentication...
            if (Request.QueryString["oauth_verifier"] != null && Session["FlickrRequestToken"] != null)
            {
                Flickr f = FlickrManager.GetInstance();

                OAuthRequestToken requestToken = Session["FlickrRequestToken"] as OAuthRequestToken;
                OAuthAccessToken accessToken = f.OAuthGetAccessToken(requestToken, Request.QueryString["oauth_verifier"]);

                FlickrManager.OAuthToken = accessToken.Token;
                FlickrManager.OAuthTokenSecret = accessToken.TokenSecret;

                lblTest.Text = "You successfully authenticated as " + accessToken.Username;

                SetAttributeValue("FlickrAuthToken", accessToken.Token);
                SetAttributeValue("FlickrAuthTokenSecret", accessToken.TokenSecret);
                SetAttributeValue("FlickrUserId", accessToken.UserId);
                SetAttributeValue("FlickrUsername", accessToken.Username);

                SaveAttributeValues(CurrentPersonId);
            }

            string FlickrUserId = GetAttributeValue("FlickrUserId");

            if (FlickrUserId.Length > 0)
            {
                btnLogin.Visible = false;
                lblTest.Text = "Welcome Back, " + GetAttributeValue("FlickrUsername");
                UserIdTextBox.Text = FlickrUserId;
                List<Photo> myPhotos = GetPhotostream(FlickrUserId);

                DataTable myDT = new DataTable();
                DataTable myDT2 = new DataTable();

                myDT.Columns.Add("RowNum");
                myDT.Columns.Add("IndicatorClass");
                myDT.Columns.Add("InnerClass");
                myDT.Columns.Add("PhotoId");
                myDT.Columns.Add("WebUrl");
                myDT.Columns.Add("Title");
                myDT.Columns.Add("LargeSquareThumbnailUrl");
                myDT.Columns.Add("Medium640Url");

                

                int RowNum = 0;

                foreach (Photo photo in myPhotos)
                {
                    DataRow thisRow = myDT.NewRow();
                    thisRow["RowNum"] = RowNum;
                    if (RowNum == 0)
                    {
                        thisRow["IndicatorClass"] = "class=\"active\"";
                        thisRow["InnerClass"] = "item active";
                    }
                    else
                    {
                        thisRow["IndicatorClass"] = "";
                        thisRow["InnerClass"] = "item";
                    }

                    thisRow["PhotoId"] = photo.PhotoId;
                    thisRow["WebUrl"] = photo.WebUrl;
                    thisRow["Title"] = photo.Title;
                    thisRow["LargeSquareThumbnailUrl"] = photo.LargeSquareThumbnailUrl;
                    thisRow["Medium640Url"] = photo.Medium640Url;
                    myDT.Rows.Add(thisRow);

                    RowNum++;
                }

                myDT2 = myDT.Copy();

                rptIndicators.DataSource = myDT;
                rptIndicators.DataBind();

                rptInnerCarousel.DataSource = myDT2;
                rptInnerCarousel.DataBind();


                //GridView1.DataSource = myDT;
                //GridView1.DataBind();
            }
            else
            {
                btnLogin.Visible = true;
            }
        }

        public static List<Photo> GetPhotostream(string userId)
        {
            
            Flickr f = FlickrManager.GetAuthInstance();
            PhotoCollection photos = f.PeopleGetPublicPhotos(userId, 0, 0, SafetyLevel.None, PhotoSearchExtras.PathAlias);
            return photos.ToList();
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            Flickr f = FlickrManager.GetInstance();
            string[] arrUrl = Request.Url.AbsoluteUri.Split(new char[] { '?' });
            string Url = arrUrl[0];
            OAuthRequestToken token = f.OAuthGetRequestToken(Url);

            Session["FlickrRequestToken"] = token;

            string url = f.OAuthCalculateAuthorizationUrl(token.Token, AuthLevel.Read);
            Response.Redirect(url);
        }
    }

    public class FlickrManager
    {
        public const string ApiKey = "c2b4c75cb56fa660fe545bec17fd9692";
        public const string SharedSecret = "eae8f8eaa17380ba";

        public static Flickr GetInstance()
        {
            return new Flickr(ApiKey, SharedSecret);
        }

        public static Flickr GetAuthInstance()
        {
            var f = new Flickr(ApiKey, SharedSecret);
            f.OAuthAccessToken = OAuthToken;
            f.OAuthAccessTokenSecret = OAuthTokenSecret;
            return f;
        }

        public static string OAuthToken
        {
            get
            {
                if (!HttpContext.Current.Request.Cookies.AllKeys.Contains("OAuthToken"))
                {
                    return null;
                }
                return HttpContext.Current.Request.Cookies["OAuthToken"].Value;
            }
            set
            {
                HttpContext.Current.Response.AppendCookie(new HttpCookie("OAuthToken", value));
            }
        }

        public static string OAuthTokenSecret
        {
            get
            {
                if (!HttpContext.Current.Request.Cookies.AllKeys.Contains("OAuthTokenSecret"))
                {
                    return null;
                }
                return HttpContext.Current.Request.Cookies["OAuthTokenSecret"].Value;
            }
            set
            {
                HttpContext.Current.Response.AppendCookie(new HttpCookie("OAuthTokenSecret", value));
            }
        }
    }
}