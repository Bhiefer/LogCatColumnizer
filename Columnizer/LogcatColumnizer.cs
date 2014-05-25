using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using System.Globalization;

namespace Columnizer
{
  /// <summary>
  /// The LogCat Columnizer parses Android logs
  /// </summary>
  public class LogCatColumnizer : ILogLineColumnizer
  {
    protected int timeOffset = 0;
    protected CultureInfo cultureInfo = new CultureInfo("en-US");
    protected const String DATETIME_FORMAT_IN = "MM-dd HH:mm:ss:fff";
    protected string lastLevel = "";
    protected string lastClass = "";
    protected string lastTime = "";

    #region ILogLineColumnizer Member

    public int GetColumnCount()
    {
      return 7;
    }

    public string[] GetColumnNames()
    {
      return new string[] {"Timestamp", "L", "Class", "Method", "Line", "Message", "TID" };
    }

    public string GetDescription()
    {
      return "Columnizer that can be used with Android LogCat logs.";
    }

    public string GetName()
    {
      return "Android LogCat";
    }

    public int GetTimeOffset()
    {
      return this.timeOffset;
    }

    /// <summary>
    /// This function has to return the timestamp of the given log line.
    /// It takes a substring of the line (between [ and ])
    /// and converts this into a DateTime object.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
    {

        if (line.Length < 18 || !Char.IsDigit(line, 0) || line[2] != '-' || line[11] != ':')
        {
            return DateTime.MinValue;
        }

        String s = line.Substring(0,18);

        // Parse into a DateTime
      
        DateTime dateTime;
        if (!DateTime.TryParseExact(s, DATETIME_FORMAT_IN, this.cultureInfo, DateTimeStyles.None, out dateTime))
        return DateTime.MinValue;

        // Add the time offset before returning
        return dateTime.AddMilliseconds(this.timeOffset);
    }

    public bool IsTimeshiftImplemented()
    {
      return true;
    }


    /// <summary>
    /// This function is called if the user changes a value in a column (edit mode in the log view).
    /// The purpose if the function is to determine a new timestamp offset. So you have to handle the
    /// call only if the given column displays a timestamp.
    /// </summary>
    public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
    {
      //if (column == 0)
      //{
      //  try
      //  {
      //    DateTime newDateTime = DateTime.ParseExact(value, DATETIME_FORMAT_OUT, this.cultureInfo);
      //    DateTime oldDateTime = DateTime.ParseExact(oldValue, DATETIME_FORMAT_OUT, this.cultureInfo);
      //    long mSecsOld = oldDateTime.Ticks / TimeSpan.TicksPerMillisecond;
      //    long mSecsNew = newDateTime.Ticks / TimeSpan.TicksPerMillisecond;
      //    this.timeOffset = (int)(mSecsNew - mSecsOld);
      //  }
      //  catch (FormatException)
      //  { }
      //}
    }

    public void SetTimeOffset(int msecOffset)
    {
      this.timeOffset = msecOffset;
    }


    /// <summary>
    /// Given a single line of the logfile this function splits the line content into columns. The function returns 
    /// a string array containing the splitted content.
    /// </summary>
    /// <remarks>
    /// This function is called by LogExpert for every line that has to be drawn in the grid view. The faster your code
    /// handles the splitting, the faster LogExpert can draw the grid view content.
    /// </remarks>
    /// <param name="callback">Callback interface with functions which can be used by the columnizer</param>
    /// <param name="line">The line content to be splitted</param>
    public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
    {
      string[] cols = new string[7] { "", "", "", "", "", "", ""};
      int start = 0, end = 0;

      // If the line is too short (i.e. does not follow the format for this columnizer) return the whole line content
      // in colum 1 (the log message column). Timestamp column will be left blank.
      if (line.Length < 18 || !Char.IsDigit(line, 0) || line[2] != '-' || line[11] != ':')
      {
          cols[0] = lastTime;
          cols[1] = lastLevel;
          cols[2] = lastClass;
      }
      else
      {
          cols[0] = line.Substring(0, 18);
          lastTime = cols[0];
          cols[1] = line.Substring(20, 1);
          lastLevel = cols[1];

          end = line.IndexOf('(', 22);
          start = line.LastIndexOf('.', end, end - 22) + 1;
          if (start == 0)
          {
              start = 22;
          }
          cols[2] = line.Substring(start, end - start);
          lastClass = cols[2];
          start = end + 1;
          end = line.IndexOf(')', start);
          cols[6] = line.Substring(start, end - start);
          start = end + 3;
      }

      end = line.LastIndexOf('[');

      if (line[line.Length - 1] == ']' && end != -1)
      {
          Boolean stackTrace = false;
          if (line.Substring(start, 5).Equals("\tat ["))
          {
              stackTrace = true;
          }
          else
          {
              cols[5] = line.Substring(start, end - start);
          }
          start = line.LastIndexOf('.', line.Length -2 , line.Length - end - 2)+1;

          if (start == 0)
          {
              start = end + 1;
          }

          end = line.LastIndexOf(':');
          cols[3] = line.Substring(start, end - start);
          cols[4] = line.Substring(end + 1, line.Length - end - 2);

          if(stackTrace)
          {
              cols[5] = "\tat " + cols[2] + '.' + cols[3] + ':' + cols[4];
          }
      }
      else
      {
          cols[5] = line.Substring(start);
      }
      

      return cols;
    }

    #endregion

    /// <summary>
    /// Implement this property to let LogExpert display the name of the Columnizer
    /// in its Colummnizer selection dialog.
    /// </summary>
    public string Text
    {
      get { return GetName(); }
    }

  }
}
