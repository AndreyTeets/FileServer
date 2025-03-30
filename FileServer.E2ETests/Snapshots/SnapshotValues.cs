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

    public static readonly string After_OpenDownloadPageNoAuth = """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "DownloadPage:"
        - table:
          - rowgroup:
            - row "Anon Path Size":
              - cell "Anon"
              - cell "Path"
              - cell "Size"
          - rowgroup:
            - row "yes anonfile1.txt 22 Download":
              - cell "yes"
              - cell "anonfile1.txt"
              - cell "22"
              - cell "Download":
                - button "Download"
        """;

    public static readonly string After_OpenDownloadPageWithAuth = """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "DownloadPage:"
        - table:
          - rowgroup:
            - row "Anon Path Size":
              - cell "Anon"
              - cell "Path"
              - cell "Size"
          - rowgroup:
            - row "yes anonfile1.txt 22 Download":
              - cell "yes"
              - cell "anonfile1.txt"
              - cell "22"
              - cell "Download":
                - button "Download"
            - row "no file1.txt 18 Download":
              - cell "no"
              - cell "file1.txt"
              - cell "18"
              - cell "Download":
                - button "Download"
        """;
}
