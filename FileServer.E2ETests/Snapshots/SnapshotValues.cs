namespace FileServer.E2ETests.Snapshots;

public static class SnapshotValues
{
    public static readonly string After_OpenInitialPage = """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        """;

    public static readonly string After_Login = """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "AuthPage:"
        - button "Logout"
        """;

    public static readonly string After_Logout = """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "AuthPage:"
        - text: "Key:"
        - textbox: "012345678912"
        - button "Login"
        """;

    public static readonly string After_UploadFileSuccessfully = """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "UploadPage:"
        - textbox
        - button "Upload" [disabled]
        - paragraph: "Uploaded: upl.test_upload_file.txt.oad"
        """;

    public static readonly string After_UploadFileWithError = """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "UploadPage:"
        - textbox: C:\fakepath\test_upload_file.txt
        - button "Upload"
        - paragraph: "Fetch error: Error: Response status: 400 . Response body: \"File with name 'upl.test_upload_file.txt.oad' already exists.\""
        """;

    public static readonly string After_OpenDownloadPage = """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "DownloadPage:"
        - table:
          - rowgroup:
            - row "Path Size":
              - cell "Path"
              - cell "Size"
          - rowgroup:
            - row "file1.txt 18 Download":
              - cell "file1.txt"
              - cell "18"
              - cell "Download":
                - button "Download"
        """;
}
