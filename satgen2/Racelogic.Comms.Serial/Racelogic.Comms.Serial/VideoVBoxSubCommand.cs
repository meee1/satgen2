namespace Racelogic.Comms.Serial;

public enum VideoVBoxSubCommand
{
	CancelFileProgress = 0,
	ReadFileInit = 1,
	WriteFileInit = 2,
	WriteFileBlock = 3,
	DeleteFile = 4,
	DirectoryListInit = 5,
	GetConfiguration = 16,
	SetConfiguration = 17,
	BlockRequest = 18,
	GetTransferStatus = 19,
	GetScreenshot = 20,
	SetOnScreenDisplayState = 21,
	RequestConfigurationOkConfirmation = 22,
	RequestSceneList = 23,
	RequestSceneName = 24,
	RequestSelectScene = 25,
	ReceivedFinishConfirmation = 26
}
