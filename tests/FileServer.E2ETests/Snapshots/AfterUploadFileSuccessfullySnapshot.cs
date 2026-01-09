namespace FileServer.E2ETests.Snapshots;

internal sealed class AfterUploadFileSuccessfullySnapshot : Snapshot
{
    protected override string Value => """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "UploadPage:"
        - button "Choose File"
        - button "Upload" [disabled]
        - button "Cancel" [disabled]
        - paragraph: "Uploaded: upl.test_upload_file.txt.oad"
        """;
}
