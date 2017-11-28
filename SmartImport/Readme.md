# Docs

time triggers for this is in  the ImportTriggers table.

In the Import Database there's an ImportSource table.
For each type of source this table is the configuration driver for csv file imports.

in other words, this program will read and understand the structure of the table and use the structure to import data from the import source files.

The code will record the imported tables in the log ImportSourceLog

Any errors will be in ImportSourceErrorLog. 
Also logged in the log folder.

For Import Source it needs and update and insert query which will be performed in this order.
The update is for records that would otherwise be replaced.

For instance for Tempo.TempoComments the pk is CommId and the update will use that as the link.
The insert will be done after.

The procedure that contains Sb.xx"UpdateInsert" will act on 2 tables from a source SI table
_backup and Service.SB.TempoXX tables where the latter is the production resource.

Deploying to 501 respectively  i.e. 87309-sb501  (prod)

