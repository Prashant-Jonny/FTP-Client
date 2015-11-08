using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FTPClient
{
    public partial class FTPViewer : Form
    {
        public string ftpAddress = string.Empty;
        public string currRemoteDir = string.Empty;
        public string currCurrentDir = string.Empty;
        public string name = string.Empty;
        public string login = string.Empty;
        public string password = string.Empty;
        public EventWaitHandle EWH = new EventWaitHandle(false, EventResetMode.ManualReset);
        public FTPViewer()
        {
            InitializeComponent();
            string[] str = Environment.GetLogicalDrives();
            foreach (string info in str)
            {
                ListViewItem lvi = new ListViewItem(info) { Tag = info };
                currentLV.Items.Add(lvi);
            }
            currentLV.FocusedItem = currentLV.Items[0]; 
            Thread T1 = new Thread(remoteLVThreadFunc);
            T1.IsBackground = true;
            EWH.Reset();
            T1.Start();
            Thread T = new Thread(currentLVThreadFunc);
            T.IsBackground = true;
            T.Start();
        }

        private void connectBttn_Click(object sender, EventArgs e)
        {
            if (addressTB.Text != string.Empty && loginTB.Text != string.Empty && passwordTB.Text != string.Empty)
            {
                lock (currRemoteDir)
                {
                    EWH.Reset();
                    remoteLV.Items.Clear();
                    remoteUp.Enabled = false;
                    ftpAddress = "ftp://" + addressTB.Text + "/";
                    login = loginTB.Text;
                    password = passwordTB.Text;
                    currRemoteDir = ftpAddress;
                    try
                    {
                        FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(currRemoteDir);
                        Request.Credentials = new NetworkCredential(login, password);
                        Request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                        Request.Proxy = null;
                        FtpWebResponse Responce = (FtpWebResponse)Request.GetResponse();
                        remoteLVUpdate();
                        EWH.Set();
                        Responce.Close();
                        connectBttn.Enabled = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                    }
                }
            }
        }

        private void comparer(List<ListViewItem> lviList, ListView lv)
        {
            lock (lv)
            {
                bool bAct = true;
                for (int i = 0; i < lv.Items.Count; i++)
                {
                    for (int j = 0; j < lviList.Count; j++)
                    {
                        string user = lviList[j].Text;
                        if (lv.Items[i].Text == user)
                        {
                            bAct = false;
                            break;
                        }
                    }
                    if (bAct == true)
                    {
                        lv.Items.RemoveAt(i);
                        i--;
                    }
                    bAct = true;
                }
                bAct = true;
                for (int i = 0; i < lviList.Count; i++)
                {
                    string user = lviList[i].Text;
                    for (int j = 0; j < lv.Items.Count; j++)
                    {
                        if (lv.Items[j].Text == user)
                        {
                            bAct = false;
                            break;
                        }
                    }
                    if (bAct == true)
                    {
                        lv.Items.Add(lviList[i]);
                        i--;
                    }
                    bAct = true;
                }
            }
        }



        private void currentLVThreadFunc()
        {
            while (true)
            {
                Thread.Sleep(2000);
                this.Invoke(new Action(delegate() { currentLVUpdate(); }));
            }
        }

        private void currentLVUpdate()
        {
            lock (currCurrentDir)
            {
                lock (currentLV)
                {
                    try
                    {
                        List<ListViewItem> lvilist = new List<ListViewItem>();
                        if (currCurrentDir != string.Empty)
                        {
                            DirectoryInfo dir = new DirectoryInfo(currCurrentDir);
                            DirectoryInfo[] str = dir.GetDirectories();
                            foreach (DirectoryInfo info in str)
                            {
                                ListViewItem lvi = new ListViewItem(info.Name) { Tag = info.FullName };
                                lvilist.Add(lvi);
                            }
                            FileInfo[] str2 = dir.GetFiles();
                            foreach (FileInfo info in str2)
                            {
                                ListViewItem lvi = new ListViewItem(info.Name) { Tag = info.FullName };
                                lvilist.Add(lvi);
                            }
                            currentUp.Enabled = true;
                        }
                        else
                        {
                            string[] str = Environment.GetLogicalDrives();
                            foreach (string info in str)
                            {
                                ListViewItem lvi = new ListViewItem(info) { Tag = info };
                                lvilist.Add(lvi);
                            }
                        }
                        comparer(lvilist, currentLV);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                    }
                }
            }
        }

        private void currentUp_Click(object sender, EventArgs e)
        {
            lock (currCurrentDir)
            {
                DirectoryInfo dir = new DirectoryInfo(currCurrentDir);
                if (dir.Parent == null)
                {
                    currCurrentDir = string.Empty;
                    currentUp.Enabled = false;
                }
                else
                {
                    currCurrentDir = dir.Parent.FullName;
                }
                currentLVUpdate();
                currentLV.FocusedItem = currentLV.Items[0];
            }
        }

        private void currentLV_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            lock (currCurrentDir)
            {
                DirectoryInfo dir = new DirectoryInfo(currentLV.FocusedItem.Tag.ToString());
                if (dir.Parent != null)
                {
                    FileInfo[] str = dir.Parent.GetFiles();
                    foreach (FileInfo info in str)
                    {
                        if (currentLV.FocusedItem.Text == info.Name)
                            return;
                    }
                }
                currCurrentDir = currentLV.FocusedItem.Tag.ToString();
                currentLVUpdate();
            }
        }

       
        
        private void remoteLVThreadFunc()
        {
            while (EWH.WaitOne())
            {
                Thread.Sleep(2000);
                this.Invoke(new Action(delegate() { remoteLVUpdate(); }));
            }
        }

        private void remoteLVUpdate()
        {
         lock (currRemoteDir)
            {
                lock (remoteLV)
                {
                    try
                    {
                        FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(currRemoteDir);
                        Request.Credentials = new NetworkCredential(login, password);
                        Request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                        Request.Proxy = null;
                        FtpWebResponse Responce = (FtpWebResponse)Request.GetResponse();
                        Stream stream = Responce.GetResponseStream();
                        StreamReader streamReader = new StreamReader(stream, Encoding.Default);
                        string ans = streamReader.ReadToEnd().Replace("\r\n", "\n");
                        string[] answer = ans.Split('\n');
                        streamReader.Close();
                        List<ListViewItem> lviList = new List<ListViewItem>();
                        for (int i = 2; i < answer.Length - 1; i++)
                        {
                            string name = string.Empty;
                            string[] parts = answer[i].Split(' ');
                            for (int j = 23; j < parts.Length; j++)
                            {
                                name += parts[j];
                                if (j != parts.Length - 1)
                                    name += ' ';
                            }
                            ListViewItem lvi = new ListViewItem(name) { Tag = currRemoteDir };
                            lviList.Add(lvi);
                        }
                        comparer(lviList, remoteLV); 
                        Responce.Close();
                        stream.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                    }
                }
            }
        }

        private void remoteLV_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            lock (currRemoteDir)
            {
                try
                {
                    FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(currRemoteDir);
                    Request.Credentials = new NetworkCredential(login, password);
                    Request.Method = WebRequestMethods.Ftp.ListDirectory;
                    Request.Proxy = null;
                    try
                    {
                        FtpWebResponse Responce = (FtpWebResponse)Request.GetResponse();
                        Stream stream = Responce.GetResponseStream();
                        StreamReader streamReader = new StreamReader(stream, Encoding.Default);
                        string ans = streamReader.ReadToEnd().Replace("\r\n", "\n");
                        string[] answer = ans.Split('\n');
                        streamReader.Close();
                        Responce.Close();
                        stream.Close();
                        for (int i = 0; i < answer.Length; i += 2)
                        {
                            if (remoteLV.FocusedItem.Text == answer[i])
                                return;
                        }
                    }
                    catch (WebException ex)
                    {
                        FtpWebResponse Responce = (FtpWebResponse)ex.Response;
                        if (Responce.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                    }
                    currRemoteDir = remoteLV.FocusedItem.Tag.ToString() + remoteLV.FocusedItem.Text + '/';
                    remoteLVUpdate();
                    remoteUp.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                }
            }
        }

        private void remoteUp_Click(object sender, EventArgs e)
        {
            lock (currRemoteDir)
            {
                string[] newDir = currRemoteDir.Split('/');
                currRemoteDir = string.Empty;
                for (int i = 0; i < newDir.Length - 2; i++)
                {
                    currRemoteDir += newDir[i] + '/';
                }
                if (currRemoteDir == ftpAddress)
                    remoteUp.Enabled = false;
                remoteLVUpdate();
            }
        }



        private void deleteDirectory(string remoteDirectory)
        {
            lock (remoteLV)
            {
                try
                {
                    FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(remoteDirectory);
                    Request.Credentials = new NetworkCredential(login, password);
                    Request.Method = WebRequestMethods.Ftp.ListDirectory;
                    Request.Proxy = null;
                    FtpWebResponse Responce;
                    Stream stream;
                    StreamReader streamReader;
                    string ans;
                    string[] files = new string[0];
                    try
                    {
                        Responce = (FtpWebResponse)Request.GetResponse();
                        stream = Responce.GetResponseStream();
                        streamReader = new StreamReader(stream, Encoding.Default);
                        ans = streamReader.ReadToEnd().Replace("\r\n", "\n");
                        files = ans.Split('\n');
                        streamReader.Close();
                        Responce.Close();
                        stream.Close();
                        for (int i = 0; i < files.Length - 1; i++)
                        {
                            Request = (FtpWebRequest)WebRequest.Create(remoteDirectory + files[i]);
                            Request.Credentials = new NetworkCredential(login, password);
                            Request.Method = WebRequestMethods.Ftp.DeleteFile;
                            Request.Proxy = null;
                            Responce = (FtpWebResponse)Request.GetResponse();
                        }
                        streamReader.Close();
                        Responce.Close();
                        stream.Close();
                    }
                    catch (WebException ex)
                    {
                        Responce = (FtpWebResponse)ex.Response;
                        if (Responce.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                    }
                    Request = (FtpWebRequest)WebRequest.Create(remoteDirectory);
                    Request.Credentials = new NetworkCredential(login, password);
                    Request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                    Request.Proxy = null;
                    Responce = (FtpWebResponse)Request.GetResponse();
                    stream = Responce.GetResponseStream();
                    streamReader = new StreamReader(stream);
                    ans = streamReader.ReadToEnd().Replace("\r\n", "\n");
                    string[] directories = ans.Split('\n');
                    streamReader.Close();
                    List<string> lviList = new List<string>();
                    for (int i = 2; i < directories.Length - 1; i++)
                    {
                        string name = string.Empty;
                        string[] parts = directories[i].Split(' ');
                        for (int j = 23; j < parts.Length; j++)
                        {
                            name += parts[j];
                            if (j != parts.Length - 1)
                                name += ' ';
                        }
                        lviList.Add(name);
                    }
                    Responce.Close();
                    stream.Close();
                    for (int i = 0; i < lviList.Count; i++)
                    {
                        deleteDirectory(remoteDirectory + lviList[i] + '/');
                    }
                    Request = (FtpWebRequest)WebRequest.Create(remoteDirectory.Substring(0,remoteDirectory.Length-1));
                    Request.Credentials = new NetworkCredential(login, password);
                    Request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                    Request.Proxy = null;
                    Responce = (FtpWebResponse)Request.GetResponse();
                    Responce.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                }
            }
        }

        private void downloadDirectory(string createDir, string remoteDirectory)
        {
            lock (remoteLV)
            {
                try
                {
                    Directory.CreateDirectory(createDir);
                    FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(remoteDirectory);
                    Request.Credentials = new NetworkCredential(login, password);
                    Request.Method = WebRequestMethods.Ftp.ListDirectory;
                    Request.Proxy = null;
                    FtpWebResponse Responce;
                    Stream stream;
                    StreamReader streamReader;
                    string ans;
                    string[] files = new string[0];
                    try
                    {
                        Responce = (FtpWebResponse)Request.GetResponse();
                        stream = Responce.GetResponseStream();
                        streamReader = new StreamReader(stream, Encoding.Default);
                        ans = streamReader.ReadToEnd().Replace("\r\n", "\n");
                        files = ans.Split('\n');
                        streamReader.Close();
                        Responce.Close();
                        stream.Close();
                        for (int i = 0; i < files.Length - 1; i++)
                        {
                            Request = (FtpWebRequest)WebRequest.Create(remoteDirectory + files[i]);
                            Request.Credentials = new NetworkCredential(login, password);
                            Request.Method = WebRequestMethods.Ftp.DownloadFile;
                            Request.Proxy = null;
                            Responce = (FtpWebResponse)Request.GetResponse();
                            FileStream outputStream = new FileStream(createDir + "\\" + files[i], FileMode.Create);
                            stream = Responce.GetResponseStream();
                            int bufferSize = 1024;
                            int readCount;
                            byte[] buffer = new byte[bufferSize];

                            readCount = stream.Read(buffer, 0, bufferSize);
                            while (readCount > 0)
                            {
                                outputStream.Write(buffer, 0, readCount);
                                readCount = stream.Read(buffer, 0, bufferSize);
                            }
                            outputStream.Close();
                        }
                        streamReader.Close();
                        Responce.Close();
                        stream.Close();
                    }
                    catch (WebException ex)
                    {
                        Responce = (FtpWebResponse)ex.Response;
                        if (Responce.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                    }
                    Request = (FtpWebRequest)WebRequest.Create(remoteDirectory);
                    Request.Credentials = new NetworkCredential(login, password);
                    Request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                    Request.Proxy = null;
                    Responce = (FtpWebResponse)Request.GetResponse();
                    stream = Responce.GetResponseStream();
                    streamReader = new StreamReader(stream);
                    ans = streamReader.ReadToEnd().Replace("\r\n", "\n");
                    string[] directories = ans.Split('\n');
                    streamReader.Close();
                    List<string> lviList = new List<string>();
                    for (int i = 2; i < directories.Length - 1; i++)
                    {
                        string name = string.Empty;
                        string[] parts = directories[i].Split(' ');
                        for (int j = 23; j < parts.Length; j++)
                        {
                            name += parts[j];
                            if (j != parts.Length - 1)
                                name += ' ';
                        }
                        lviList.Add(name);
                    }
                    Responce.Close();
                    stream.Close();
                    for (int i = 0; i < lviList.Count; i++)
                    {
                        for (int j = 0; j < files.Length - 1; j++)
                        {
                            if (lviList[i] == files[j])
                            {
                                lviList.RemoveAt(i);
                                i--;
                                break;
                            }
                        }
                    }
                    for (int i = 0; i < lviList.Count; i++)
                    {
                        downloadDirectory(createDir + "\\" + lviList[i], remoteDirectory + lviList[i] + '/');
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                }
            }
        }

        #region ContextMenu

        private void createDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Renaming r = new Renaming();
            r.f = this;
            r.Text = "New directory";
            r.ShowDialog();
            if (name == string.Empty)
                return;
            for(int i=0;i<remoteLV.Items.Count;i++)
            {
                if(name == remoteLV.Items[i].Text)
                {
                    MessageBox.Show("The directory with such a name already exists.","Error",MessageBoxButtons.OK);
                    return;
                }
            }
            lock (currRemoteDir)
            {
                try
                {
                    FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(currRemoteDir + name);
                    Request.Credentials = new NetworkCredential(login, password);
                    Request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    Request.Proxy = null;
                    FtpWebResponse Responce = (FtpWebResponse)Request.GetResponse();
                    Stream stream = Responce.GetResponseStream();
                    Responce.Close();
                    name = string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                }
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Renaming r = new Renaming();
            r.f = this;
            r.Text = "Renaming";
            r.ShowDialog();
            if (name == string.Empty)
                return;
            for (int i = 0; i < remoteLV.Items.Count; i++)
            {
                if (name == remoteLV.Items[i].Text)
                {
                    MessageBox.Show("The directory or file with such a name already exists.", "Error", MessageBoxButtons.OK);
                    return;
                }
            }
            lock (currRemoteDir)
            {
                try
                {
                    FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(currRemoteDir + remoteLV.FocusedItem.Text);
                    Request.Credentials = new NetworkCredential(login, password);
                    Request.Method = WebRequestMethods.Ftp.Rename;
                    Request.Proxy = null;
                    Request.RenameTo = name;
                    FtpWebResponse Responce = (FtpWebResponse)Request.GetResponse();
                    name = string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (currRemoteDir)
            {
                try
                {
                    FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(currRemoteDir);
                    Request.Credentials = new NetworkCredential(login, password);
                    Request.Method = WebRequestMethods.Ftp.ListDirectory;
                    Request.Proxy = null;
                    FtpWebResponse Responce = (FtpWebResponse)Request.GetResponse();
                    Stream stream = Responce.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream, Encoding.Default);
                    string ans = streamReader.ReadToEnd().Replace("\r\n", "\n");
                    string[] answer = ans.Split('\n');
                    streamReader.Close();
                    Responce.Close();
                    stream.Close();
                    for (int i = 0; i < answer.Length - 1; i++)
                    {
                        if (remoteLV.FocusedItem.Text == answer[i])
                        {
                            Request = (FtpWebRequest)WebRequest.Create(currRemoteDir + remoteLV.FocusedItem.Text);
                            Request.Credentials = new NetworkCredential(login, password);
                            Request.Method = WebRequestMethods.Ftp.DeleteFile;
                            Request.Proxy = null;
                            Responce = (FtpWebResponse)Request.GetResponse();
                            break;
                        }
                        if (i == answer.Length - 2)
                        {
                            deleteDirectory(currRemoteDir + remoteLV.FocusedItem.Text + '/');
                        }
                    }
                    streamReader.Close();
                    Responce.Close();
                    stream.Close();
                    lock (remoteLV)
                    {
                        remoteLV.Items.Remove(remoteLV.FocusedItem);
                    }
                }
                catch (WebException ex)
                {
                    FtpWebResponse Responce = (FtpWebResponse)ex.Response;
                    if (Responce.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        deleteDirectory(currRemoteDir + remoteLV.FocusedItem.Text + '/');
                    }
                    else
                        MessageBox.Show(ex.Message + "\n" + Responce.StatusCode.ToString() , "Error", MessageBoxButtons.OK);
                }
            }
        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(currCurrentDir == string.Empty)
            {
                MessageBox.Show("Choose the directory on the current computer.", "Error", MessageBoxButtons.OK);
                return;
            }
            lock (currRemoteDir)
            {
                try
                {
                    FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(currRemoteDir);
                    Request.Credentials = new NetworkCredential(login, password);
                    Request.Method = WebRequestMethods.Ftp.ListDirectory;
                    Request.Proxy = null;
                    
                    FtpWebResponse Responce = (FtpWebResponse)Request.GetResponse();
                    Stream stream = Responce.GetResponseStream();
                    
                    StreamReader streamReader = new StreamReader(stream, Encoding.Default);

                    string ans = streamReader.ReadToEnd().Replace("\r\n", "\n");
                    
                    string[] answer = ans.Split('\n');
                    streamReader.Close();
                    Responce.Close();
                    stream.Close();
                    for (int i = 0; i < answer.Length - 1; i++)
                    {
                        if (remoteLV.FocusedItem.Text == answer[i])
                        {
                            Request = (FtpWebRequest)WebRequest.Create(currRemoteDir + remoteLV.FocusedItem.Text);
                            Request.Credentials = new NetworkCredential(login, password);
                            Request.Method = WebRequestMethods.Ftp.DownloadFile;
                            Request.Proxy = null;
                            Responce = (FtpWebResponse)Request.GetResponse();
                            FileStream outputStream = new FileStream(currCurrentDir + "\\" + remoteLV.FocusedItem.Text, FileMode.Create);
                            stream = Responce.GetResponseStream();
                            int bufferSize = 1024;
                            int readCount;
                            byte[] buffer = new byte[bufferSize];

                            readCount = stream.Read(buffer, 0, bufferSize);
                            while (readCount > 0)
                            {
                                outputStream.Write(buffer, 0, readCount);
                                readCount = stream.Read(buffer, 0, bufferSize);
                            }
                            outputStream.Close();
                            break;
                        }
                        if (i == answer.Length - 2)
                        {
                            downloadDirectory(currCurrentDir + "\\" + remoteLV.FocusedItem.Text, currRemoteDir + remoteLV.FocusedItem.Text + '/');
                        }
                    }
                    streamReader.Close();
                    Responce.Close();
                    stream.Close();
                }
                catch (WebException ex)
                {
                    FtpWebResponse Responce = (FtpWebResponse)ex.Response;
                    if (Responce.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        downloadDirectory(currCurrentDir + "\\" + remoteLV.FocusedItem.Text, currRemoteDir + remoteLV.FocusedItem.Text + '/');
                    }
                    else
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                }
            }
        }

        #endregion
    }
}
