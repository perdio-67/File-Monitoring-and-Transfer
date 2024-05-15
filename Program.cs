// See https://aka.ms/new-console-template for more information
using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;

[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

[DllImport("user32.dll")]
static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
const int SW_HIDE = 0;
const int SW_SHOW = 5;
const int SW_MIN = 6;

FileSystemWatcher _watchFolder = new FileSystemWatcher();
string copy_delay = ConfigurationManager.AppSettings.Get("copy_delay");
string patterns_path = ConfigurationManager.AppSettings.Get("patterns_path");
string submissions_path = ConfigurationManager.AppSettings.Get("submissions_path");
int delay = 0;


bool result;
var mutex = new System.Threading.Mutex(true, "Submission_Console", out result);

string myfile = @AppContext.BaseDirectory + @"\logs\log "+DateTime.Now.ToString("dd-MM-yyyy")+".txt";
int pattern_count = 0;

Boolean _logging_busy = false;

if (!result)
{
    Console.WriteLine(DateTime.Now + ": Another instance is already running.");
    Console.WriteLine("\n"+ DateTime.Now + ": Enter any key to exit.");
    Console.ReadKey();
    return;
}

GC.KeepAlive(mutex);
var handle = GetConsoleWindow();


try {


    //creating logs folder
    System.IO.Directory.CreateDirectory(@AppContext.BaseDirectory + @"\logs");

    //logging
    logging_lines(myfile);
    logging("Intilizing..", myfile);


    //getting delay value
    delay = 1000 * int.Parse(copy_delay);
    //start folder monitoring
    startActivityMonitoring(patterns_path);
    //Hide console
    ShowWindow(handle, SW_MIN);
    Console.ReadLine();

}
catch (Exception ex) {
    Console.WriteLine(DateTime.Now + ": Error Intilizing " + ex.ToString());
    logging("Error Intilizing: " + ex.ToString(), myfile);
}



void startActivityMonitoring(string sPath)
{   
    
    try
    {
    logging("Starting to Monitor..", myfile);
    // This is the path we want to monitor
    Console.WriteLine("KEEP THIS SERVICE RUNNING AT ALL TIMES!");
    Console.WriteLine("");
    Console.WriteLine(DateTime.Now + ": Intilizing..");
    _watchFolder.Path = sPath;

    // Make sure you use the OR on each Filter because we need to monitor
    // all of those activities

    _watchFolder.NotifyFilter = System.IO.NotifyFilters.DirectoryName;

    _watchFolder.NotifyFilter =
    _watchFolder.NotifyFilter | System.IO.NotifyFilters.FileName;
    _watchFolder.NotifyFilter =
    _watchFolder.NotifyFilter | System.IO.NotifyFilters.Attributes;

    // Now hook the triggers(events) to our handler (eventRaised)
    //_watchFolder.Changed += new FileSystemEventHandler(eventRaised);
    _watchFolder.Created += new FileSystemEventHandler(eventCreated);
    //_watchFolder.Deleted += new FileSystemEventHandler(eventRaised);

    // Occurs when a file or directory is renamed in the specific path
    //_watchFolder.Renamed += new System.IO.RenamedEventHandler(eventRenameRaised);

    // And at last.. We connect our EventHandles to the system API (that is all
    // wrapped up in System.IO)

        _watchFolder.EnableRaisingEvents = true;

        Console.WriteLine(DateTime.Now + ": Folder Monitoring on: " +sPath);
        Console.WriteLine(DateTime.Now + ": Target Folder: " + submissions_path);
        Console.WriteLine(DateTime.Now + ": Started..");


        logging("Folder Monitoring on: " + sPath, myfile);
        logging("Target Folder: " + submissions_path, myfile);
        logging("Monitoring Started.\n", myfile);
    }
    catch (Exception ee)
    {
        Console.WriteLine(DateTime.Now + ": Error Monitoring Intilizing: " + ee.Message);
        logging("Error Monitoring Intilizing: " + ee.ToString(), myfile);
    }
}

void eventCreated(object sender, System.IO.FileSystemEventArgs e)
{
    //MessageBox.Show(e.FullPath);
    String path = e.FullPath;
    Boolean _first_time = true;
        while (true)
        {
            try
            {

            if (_first_time)
            {
                Console.WriteLine(DateTime.Now + ": New Submission is being submitted");
                logging("New Pattern is being submitted", myfile);
                _first_time = false;
            }

            File.OpenRead(path);
            break;
            
        }
            catch (Exception ee)
            {
                Console.WriteLine(DateTime.Now + ": still copying....");
                logging("Pattern is still copying: " + ee.Message, myfile);
            //Debug.WriteLine(DateTime.Now + "still copying " + ee.ToString());
            //lb1.Content = "still copying " + ee.ToString();
            Thread.Sleep(100);
            }

        }
        if (path.EndsWith("jpeg", comparisonType: StringComparison.OrdinalIgnoreCase) || path.EndsWith("png", comparisonType: StringComparison.OrdinalIgnoreCase) || path.EndsWith("jpg", comparisonType: StringComparison.OrdinalIgnoreCase))
        {

            //Console.WriteLine(DateTime.Now + "Done, New File:" + path);

            //lb1.Content = string.Format("New File: " + path);
            Task.Delay(delay).ContinueWith(t => copy(path, submissions_path + "\\" + path.Substring(path.LastIndexOf("\\"))));
            //Task.Delay(0).ContinueWith(t => update_paths());


        }

}

void copy(string path_from, string path_to)
{
    try
    {
        File.Copy(path_from, path_to, true);
        Console.WriteLine(DateTime.Now + ": Copied Complete " + path_to);
        logging("Pattern was Copied Completely to " + path_to, myfile);
        pattern_count++;
        Console.WriteLine(DateTime.Now + ": Total Pattterns Copied = " + pattern_count);
        logging("Total Pattterns Copied = " + pattern_count, myfile);

    }
    catch(Exception e)
    {
        Console.WriteLine(DateTime.Now + ": Copy Error: " +e.Message);
        logging("Error Copying Pattern: " + e.Message, myfile);
    }
}

void logging(string s, string file)
{
    if (!_logging_busy)
    {
        try
        {
            _logging_busy = true;
            using (StreamWriter sw = File.AppendText(file))
            {
                sw.WriteLine(DateTime.Now + ": " + s);
                sw.Close();
            }
            _logging_busy = false;
        }
        catch (Exception exx)
        {
            _logging_busy = false;
        }
    }
    else
    {
        logging(s, file);
    }

}

void logging_lines(string file, Boolean clear = false)
{
    if (!clear)
        using (StreamWriter sw = File.AppendText(file))
        {
            sw.WriteLine("\n\n\n\n");
            sw.Close();
        }
    else
        using (StreamWriter sw = File.CreateText(file))
        {
            sw.WriteLine("\n\n\n\n");
            sw.Close();
        }

}