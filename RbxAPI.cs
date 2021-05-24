using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RobloxAPI
{
    public class RbxAPI
    {
        public List<string> ItemIDs = new List<string>();
        public Dictionary<string, string> SongData = new Dictionary<string, string>();

        public enum AssetType
        { 
            Shirt,
            Pants,
            Audio
        }

        public enum UserReturn
        {
            UserName,
            DisplayName,
            UserId,
            Created,
            Status,
            PastUserNames,
            Description
        }

        public enum AccountReturn
        {
            UserId,
            UserName,
        }

        public enum RobloxVersion
        {
            Client,
            Studio
        }

        public enum DateTimeReturn
        {
            DateTime,
            UTC,
            Date,
            NumericalDate,
            NumericalDateTime,
            NumericalDateWithTOD,
            CustomFormat,
        }
     
        private bool ValidId(string ID)
        {
            long Parsed;

            return long.TryParse(ID, out Parsed);
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        /// <summary>
        /// Returns User avatar.
        /// </summary>
        /// <param name="UserId">User ID</param>
        public string GetUserAvatar(string UserId)
        {
            string Avatar = string.Empty;

            return Avatar;
        }

        /// <summary>
        /// Returns page cursor for next page.
        /// </summary>
        /// <param name="CatalogURL">Catalouge URL</param>
        public string GetNextCatalogCursor(string CatalogURL)
        {
            string Cursor = string.Empty;

            using (WebClient client = new WebClient())
            {
                string JsonData = client.DownloadString(CatalogURL);

                JToken Data = JToken.Parse(JsonData);

                Cursor = Data["nextPageCursor"].ToString();
            }

            return Cursor;
        }

        /// <summary>
        /// Returns Asset Name.
        /// </summary>
        /// <param name="ID">Asset ID</param>
        public string GetAssetName(string ID)
        {
            string Name = string.Empty;

            if (!ValidId(ID))
                return Name;

            using (WebClient client = new WebClient())
            {
                string JsonData = client.DownloadString($"http://api.roblox.com/Marketplace/ProductInfo?assetId={ID}");

                JToken Data = JToken.Parse(JsonData);

                Name = Data["Name"].ToString();

            }

            return Name;
        }

        /// <summary>
        /// Returns fixed FileName to be saved.
        /// </summary>
        /// <param name="FileName">File name</param>
        public string GetValidName(string FileName)
        {
            return string.Join("_", FileName.Split(System.IO.Path.GetInvalidFileNameChars()));
        }

        private string ParseFile(string File)
        {
            int pfrom = 0;

            if (File.Contains("ShirtTemplate"))
                pfrom = File.IndexOf("<Content name=\"ShirtTemplate\">") + "<Content name=\"ShirtTemplate\">".Length + 15;
            else if (File.Contains("PantsTemplate"))
                pfrom = File.IndexOf("<Content name=\"PantsTemplate\">") + "<Content name=\"ShirtTemplate\">".Length + 15;

            int pto = File.IndexOf("</Content>") - 14;

            return File.Substring(pfrom, pto - pfrom).Replace("http://www.roblox.com/asset/?id=", "https://assetdelivery.roblox.com/v1/asset?id=");
        }


        /// <summary>
        /// Gathers AudioIDs of given library URL | Stored in SongData Dictionary
        /// </summary>
        /// <param name="LibraryUrl">Library URL</param>
        public void ScrapeAudioIds(string LibraryUrl)
        {
            int pfrom = LibraryUrl.IndexOf("item-image-wrapper") + "item-image-wrapper".Length + 1;
            int pto = LibraryUrl.IndexOf("PagingContainerDivTop");

            string FirstParse = LibraryUrl.Substring(pfrom, pto - pfrom);
            string[] AudioObj = FirstParse.Split(new string[] { "class=NotAPrice>Free" }, StringSplitOptions.None);

            foreach (string SData in AudioObj)
            {
                string AudioData = SData;
                if (AudioData.Contains("CatalogItemInfoLabel"))
                {
                    AudioData = ReplaceFirst(AudioData, "></span></div>", "");
                }
                if (!AudioData.Contains("data-mediathumb-url="))
                    return;

                pfrom = AudioData.IndexOf("data-mediathumb-url=") + "data-mediathumb-url=".Length;
                pto = AudioData.IndexOf("></span></div>");
                string AudioUrl = AudioData.Substring(pfrom, pto - pfrom);

                pfrom = AudioData.IndexOf("\" title=\"") + "\" title=\"".Length;
                pto = AudioData.LastIndexOf("\">");
                string Name = AudioData.Substring(pfrom, pto - pfrom);

                SongData.Add(Name, AudioUrl);
            }
        }

        /// <summary>
        /// Gathers IDs of assets from given URL | Stored in ItemIDs
        /// </summary>
        /// <param name="CatalogURL">Catalog URL</param>
        public void GatherIDs(string CatalogURL)
        {
            using (WebClient client = new WebClient())
            {
                string JsonData = client.DownloadString(CatalogURL);

                JToken Data = JToken.Parse(JsonData);
                JArray IdData = JArray.Parse(Data["data"].ToString());

                for (int i = 0; i < IdData.Count; i++)
                {
                    Data = JToken.Parse(IdData[i].ToString());
                    ItemIDs.Add(Data["id"].ToString());
                }
            }
        }

        private async void FetchIDs(AssetType TypeSearch, int PageCount = 1)
        {
            string BaseUrl;
            string CatalogUrl;

            switch (TypeSearch)
            {
                case AssetType.Shirt:
                    BaseUrl = $"https://catalog.roblox.com/v1/search/items?Keyword={TypeSearch}&category=Clothing&limit=100&subcategory=Shirts";
                    CatalogUrl = $"https://catalog.roblox.com/v1/search/items?Keyword={TypeSearch}&category=Clothing&limit=100&subcategory=Shirts";
                    for (int i = 1; i <= (PageCount); i++)
                    {
                        if (i == 1)
                        {
                            GatherIDs(CatalogUrl);
                        }
                        else
                        {
                            CatalogUrl = BaseUrl + $"&cursor={GetNextCatalogCursor(CatalogUrl)}";

                            GatherIDs(CatalogUrl);
                        }
                    }
                    while (ItemIDs.Count != (PageCount))
                    {
                        await Task.Delay(25);
                    }

                    break;
                case AssetType.Pants:
                    BaseUrl = $"https://catalog.roblox.com/v1/search/items?Keyword={TypeSearch}&category=Clothing&limit=100&subcategory=Pants";
                    CatalogUrl = $"https://catalog.roblox.com/v1/search/items?Keyword={TypeSearch}&category=Clothing&limit=100&subcategory=Pants";
                    for (int i = 1; i <= (PageCount); i++)
                    {
                        if (i == 1)
                        {
                            GatherIDs(CatalogUrl);
                        }
                        else
                        {
                            CatalogUrl = BaseUrl + $"&cursor={GetNextCatalogCursor(CatalogUrl)}";

                            GatherIDs(CatalogUrl);
                        }
                    }
                    while (ItemIDs.Count != (PageCount))
                    {
                        await Task.Delay(25);
                    }

                    break;
                case AssetType.Audio:
                    for (int i = 1; i <= (PageCount); i++)
                    {
                        using (WebClient client = new WebClient())
                        {
                            Console.WriteLine($"https://search.roblox.com/catalog/contents?CatalogContext=2&SortAggregation=5&LegendExpanded=true&Category=9&PageNumber={i}&Keyword={TypeSearch}");
                            ScrapeAudioIds(client.DownloadString($"https://search.roblox.com/catalog/contents?CatalogContext=2&SortAggregation=5&LegendExpanded=true&Category=9&PageNumber={i}&Keyword={TypeSearch}"));
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Returns Audio Raw URL
        /// </summary>
        /// <param name="AudioID">Get audio download URL</param>
        public string GetAudioUrl(string AudioID)
        {
            string Url = string.Empty;

            if (!ValidId(AudioID))
                return Url;

            using (WebClient client = new WebClient())
            {
                string AudioData = client.DownloadString($"https://www.roblox.com/library/{AudioID}/#");

                int pfrom = AudioData.IndexOf("data-mediathumb-url=") + "data-mediathumb-url=".Length;
                int pto = AudioData.IndexOf("></span></div>");
                Url = AudioData.Substring(pfrom, pto - pfrom);
            }
            return Url;
        }

        /// <summary>
        /// Gets specified userdata.
        /// </summary>
        /// <param name="UserId">UserId of Player</param>
        /// <param name="ToReturn">Account data to return.</param>
        public string GetUserData(string UserId, UserReturn ToReturn)
        {
            string RequestedData = string.Empty;

            if (!ValidId(UserId))
                return RequestedData;

            using (WebClient client = new WebClient())
            {
                string JsonData = client.DownloadString($"https://users.roblox.com/v1/users/{UserId}");

                JToken Data = JToken.Parse(JsonData);

                switch (ToReturn)
                {
                    case UserReturn.UserName:
                        RequestedData = Data["name"].ToString();
                        break;
                    case UserReturn.DisplayName:
                        RequestedData = Data["displayName"].ToString();
                        break;
                    case UserReturn.UserId:
                        RequestedData = Data["id"].ToString();
                        break;
                    case UserReturn.Created:
                        RequestedData = Data["created"].ToString();
                        break;
                    case UserReturn.Status:
                        JsonData = client.DownloadString($"https://users.roblox.com/v1/users/{UserId}/status");
                        Data = JToken.Parse(JsonData);
                        RequestedData = Data["status"].ToString();
                        break;
                    case UserReturn.PastUserNames:
                        break;
                    case UserReturn.Description:
                        RequestedData = Data["description"].ToString();
                        break;
                }
            }

            return RequestedData;
        }


        /// <summary>
        /// Gets UserID from name or vice versa.
        /// </summary>
        /// <param name="ToReturn">Account data to return.</param>
        /// <param name="UserData">Account UserData : UserId|UserName</param>
        public string GetNameOrId(AccountReturn ToReturn, string UserData)
        {
            string Data = string.Empty;
            using (WebClient client = new WebClient())
            {
                string JsonData;
                JToken JData;

                switch (ToReturn)
                {
                    case AccountReturn.UserId:
                        JsonData = client.DownloadString($"https://api.roblox.com/users/get-by-username?username={UserData}");
                        JData = JToken.Parse(JsonData);
                        Data = JData["Username"].ToString();

                        break;
                    case AccountReturn.UserName:
                        if (!ValidId(UserData))
                            return Data;

                        JsonData = client.DownloadString($"https://api.roblox.com/users/{UserData}");
                        JData = JToken.Parse(JsonData);
                        Data = JData["Username"].ToString();

                        break;
                }
            }
            return Data;
        }


        /// <summary>
        /// Gets version data of roblox
        /// </summary>
        /// <param name="version">Version you'd like to get: Default Client</param>
        public string GetRobloxVersion(RobloxVersion version = RobloxVersion.Client)
        {
            switch (version)
            {
                case RobloxVersion.Client:
                    return new WebClient().DownloadString("http://setup.roblox.com/version");
                case RobloxVersion.Studio:
                    return new WebClient().DownloadString("http://setup.roblox.com/versversionStudio");
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets a user's HeadShot
        /// </summary>
        /// <param name="UserId">UserId of Player</param>
        public string GetUserHeadShot(string UserId)
        { 
            string Data = string.Empty;

            if (!ValidId(UserId))
                return Data;

            using (WebClient client = new WebClient())
            {
                string JsonData = client.DownloadString($"https://www.roblox.com/headshot-thumbnail/json?userId={UserId}&width=48&height=48");

                JToken JData = JToken.Parse(JsonData);

                Data = JData["Url"].ToString();
            }

            return Data;
        }

        /// <summary>
        /// Gets a users last online date and time.
        /// </summary>
        /// <param name="UserId">UserId of Player</param>
        /// <param name="TypeReturn">What DateTime format you'd like to recieve: Default UTC</param>
        /// <param name="CustomFormat">Format if specified in TypeReturn</param>
        public string GetLastOnline(string UserId, DateTimeReturn TypeReturn = DateTimeReturn.UTC, string CustomFormat = null)
        {
            string Data = string.Empty;

            Console.WriteLine(TypeReturn);

            if (!ValidId(UserId))
                return Data;

            using (WebClient client = new WebClient())
            {
                string JsonData = client.DownloadString($"http://api.roblox.com/users/{UserId}/onlinestatus/");

                JToken JData = JToken.Parse(JsonData);

                Data = JData["LastOnline"].ToString();
            }

            switch (TypeReturn)
            {
                case DateTimeReturn.Date:
                    return DateTime.Parse(Data).ToString("dddd, MMMM dd yyyy");
                case DateTimeReturn.UTC:
                    return System.Xml.XmlConvert.ToString(DateTime.Parse(Data), System.Xml.XmlDateTimeSerializationMode.Utc);
                case DateTimeReturn.DateTime:
                    return DateTime.Parse(Data).ToString("dddd, MMMM dd yyyy hh:mm tt");
                case DateTimeReturn.NumericalDate: 
                    return DateTime.Parse(Data).ToString("MM/dd/yyyy");
                case DateTimeReturn.NumericalDateTime:
                    return DateTime.Parse(Data).ToString("MM/dd/yyyy H:mm");
                case DateTimeReturn.NumericalDateWithTOD:
                    return DateTime.Parse(Data).ToString("MM/dd/yyyy H:mm tt");
                case DateTimeReturn.CustomFormat:
                    try {
                        return DateTime.Parse(Data).ToString(CustomFormat);
                    }catch{return Data;}
            }

            return Data;
        }
    }
}
