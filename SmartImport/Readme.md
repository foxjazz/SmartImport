# Docs

In the Import Database there's an ImportSource table.
For each type of source this table is the configuration driver for csv file imports.

The code will record the imported tables in the log ImportSourceLog

Any errors will be in ImportSourceErrorLog. 
