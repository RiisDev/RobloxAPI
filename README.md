# RobloxAPI
C# Roblox API


```cs
GetLastOnline(string UserId, DateTimeReturn TypeReturn = DateTimeReturn.UTC, string CustomFormat = null)
GetUserHeadShot(string UserId)
GetRobloxVersion(RobloxVersion version = RobloxVersion.Client)
GetNameOrId(AccountReturn ToReturn, string UserData)
GetUserData(string UserId, UserReturn ToReturn)
GetAudioUrl(string AudioID)
GatherIDs(string CatalogURL)
ScrapeAudioIds(string LibraryUrl)
GetValidName(string FileName)
GetAssetName(string ID)
GetNextCatalogCursor(string CatalogURL)
GetUserAvatar(string UserId)

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
```
