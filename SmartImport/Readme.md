# Docs

In the Import Database there's an ImportSource table.
For each type of source this table is the configuration driver for csv file imports.

The code will record the imported tables in the log ImportSourceLog

Any errors will be in ImportSourceErrorLog. 

For Import Source it needs and update and insert query which will be performed in this order.
The update is for records that would otherwise be replaced.

For instance for Tempo.TempoComments the pk is CommId and the update will use that as the link.
The insert will be done after.
