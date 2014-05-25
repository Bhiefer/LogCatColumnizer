LogCatColumnizer
================

Columnizer for LogExpert. Parses Android logcat logs. 

It is also capable to parse our specific logs that includes method name and line. It should have following format:

12-31 17:26:46.268: &lt;level&gt;/&lt;class tag&gt;(&lt;TID&gt;): &lt;log message&gt;[&lt;method&gt;():&lt;line&gt;]

Example:
12-31 17:26:46.268: D/MediaDownloader(3012): Track wasn't changed[add():159]

Class tag can be logged as full class name (to be able do the deobfuscation). Columnizer shows only the simple name.


Example:

com.company.foo.SomeClass is shown as SomeClass only
