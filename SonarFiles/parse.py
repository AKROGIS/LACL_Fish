# -*- coding: utf-8 -*-

"""
Reads a file (or folder of files recursively) of Sonar data from a
XXX Hardware/software data collector
and stores the file (as a blob) and all of the file's data in three
tables in a relational database.  This file assumes the tables in the
database have been created (see build_db.sql).

You will need to edit the end of this file to specify which file or folder
to process, as well as additional parameters as described here.

This process can be run in a **test** mode to look for errors or unexpected
data in the input file(s), and/or **save** mode to write the data to the
database.  Use the `test` and `save` parameters to the CONFIG object.

It is recommend to always test a file/folder before running in save mode.
If errors are found, either remove the offending files (if they are bad),
or modify this script to deal with the unexpected file data.  Most tests
assumptions are listed in the GLOBAL variables at the start of this file.

This script is hard coded to assume the database backend is SQL Server.
The server name and database can be specified by `server` and `database`
parameters to the CONFIG object.

This script was written for python 2.7 and tested succesfully with python 3.6
It has an external dependency on the **pyodbc** python module.
It can be installed with `pip install pyodbc`

NOTE: with py3 and unicode_literals (py2) all strings are unicode.  However,
the database fields are all 8bit characters (not unicode), except for the
filename.  This is acceptable/appropriate because the text in the datafiles is
limited to ASCII.  If this changes, the database schema will need to change.
"""

from __future__ import absolute_import, division, print_function, unicode_literals

from io import open
import os
import sys

CONFIG = {
    # source - A single sonar file, or a folder of sonar files.
    "source": "C:/tmp/LACL Sonar Counts 2020",
    # test - If True then we test the input files for unexpected formatting.
    "test": True,
    # save - If False, then do not save the data files/summary into the database.
    "save": False,
    # server - the host name of the database server to write to when save = True.
    "server": "inpakrovmais",
    # database - the name of the database to write to when save = True.
    "database": "LACL_Fish",
}

# column names for the tabular data in the middle of the file
# the following variations were seen in the initial data load.
# new variants can be added as needed, but may require adding
# additional columns to the database
# fmt: off
BODY_HEADER_1 = ["File", "Total", "Mark", "Frame#", "Dir", "R (m)", "Theta", "L(cm)",   "T(cm)", "L/T",  "Aspect", "Cluster", "Time", "Date", "Latitude", "Longitude", "Pan", "Tilt", "Roll"]
BODY_HEADER_2 = ["File", "Total", "Mark", "Frame#", "Dir", "R (m)", "Theta", "L(cm)",  "dR(cm)", "L/dR", "Aspect", "Cluster", "Time", "Date", "Latitude", "Longitude", "Pan", "Tilt", "Roll"]
BODY_HEADER_3 = ["File", "Total", "Mark", "Frame#", "Dir", "R (m)", "Theta", "L(frm)", "dR(cm)", "L/T",  "Aspect", "Track",   "Time", "Date", "Latitude", "Longitude", "Pan", "Tilt", "Roll"]
BODY_HEADER_4 = ["File", "Total", "Mark", "Frame#", "Dir", "R (m)", "Theta", "L(frm)", "dR(cm)", "L/dR", "Aspect", "Track",   "Time", "Date", "Latitude", "Longitude", "Pan", "Tilt", "Roll"]
BODY_HEADER_5 = ["File", "Total",         "Frame#", "Dir", "R (m)", "Theta", "L(cm)",   "T(cm)", "L/T",  "Aspect",            "Time", "Date", "Latitude", "Longitude", "Pan", "Tilt", "Roll", "Species", "Motion", "Q", "N", "Comment"]
BODY_HEADER_6 = ["File", "Total",         "Frame#", "Dir", "R (m)", "Theta", "L(cm)",  "dR(cm)", "L/dR", "Aspect",            "Time", "Date", "Latitude", "Longitude", "Pan", "Tilt", "Roll", "Species", "Motion", "Q", "N", "Comment"]
# fmt: on
WELL_KNOWN_HEADERS = [
    BODY_HEADER_1,
    BODY_HEADER_2,
    BODY_HEADER_3,
    BODY_HEADER_4,
    BODY_HEADER_5,
    BODY_HEADER_6,
]

# These are the names of the various key/value pairs found
# above and below the tabular section in the initial data load.
# New keys can be added as needed, but will require adding
# new columns to the database.
WELL_KNOWN_KEYS = [
    "Alpha",
    "Average N Beams",
    "Background Subtraction",
    "Cluster Area",
    "Convolve Beams",
    "Convolve Samps",
    "Count  File Name",
    "Detect Motion",
    "Downstream",
    "Echo Detect Angle",
    "Editor ID",
    "Factor A",
    "Factor C",
    "Intensity",
    "Log Multiplier",
    "Max Count Angle",
    "Max Count Range",
    "Max Process Frame",
    "Min Count Angle",
    "Min Count Range",
    "Min Process Frame",
    "Min Threshold",
    "Min Track Size",
    "Sidestream",
    "Smooth Foreground",
    "Source File Date",
    "Source File End",
    "Source File Name",
    "Source File Start",
    "TL Correction",
    "Threshold",
    "Total Fish",
    "Upstream",
    "Upstream Motion",
    "Window End",
    "Window Start",
    "body",
    "body_header",
]
FILE_KEYS = ["Count  File Name", "Source File Name"]

# This converts the key values above to valid column names.
# keys not listed are used as column names as shown above.
FIELD_NAMES = {
    "File": "File_Code",
    "R (m)": "R_m",
    "L(cm)": "L_cm",
    "L(frm)": "L_frm",
    "dR(cm)": "dR_cm",
    "L/dR": "L_over_dR",
    "T(cm)": "T_cm",
    "L/T": "L_over_T",
    "Time": "Time_iso",
    "Date": "Date_iso",
    "Q": "Quality",
    "N": "Repeat_Count",
}


def get_connection_or_die(server, database):
    """
    Get a Trusted pyodbc connection to the SQL Server database on server.

    Try several connection strings.
    See https://github.com/mkleehammer/pyodbc/wiki/Connecting-to-SQL-Server-from-Windows

    Exit with an error message if there is no successful connection.
    """
    drivers = [
        "{ODBC Driver 17 for SQL Server}",  # supports SQL Server 2008 through 2017
        "{ODBC Driver 13.1 for SQL Server}",  # supports SQL Server 2008 through 2016
        "{ODBC Driver 13 for SQL Server}",  # supports SQL Server 2005 through 2016
        "{ODBC Driver 11 for SQL Server}",  # supports SQL Server 2005 through 2014
        "{SQL Server Native Client 11.0}",  # DEPRECATED: released with SQL Server 2012
        # '{SQL Server Native Client 10.0}',    # DEPRECATED: released with SQL Server 2008
    ]
    conn_template = "DRIVER={0};SERVER={1};DATABASE={2};Trusted_Connection=Yes;"
    for driver in drivers:
        conn_string = conn_template.format(driver, server, database)
        try:
            connection = pyodbc.connect(conn_string)
            return connection
        except pyodbc.Error:
            pass
    print("Rats!! Unable to connect to the database.")
    print("Make sure you have an ODBC driver installed for SQL Server")
    print("and your AD account has the proper DB permissions.")
    print("Contact akro_gis_helpdesk@nps.gov for assistance.")
    sys.exit()


def parse_header(file_handle, file_data):
    """
    Reads header lines from file_handle and puts important finds in file_data.

    Most header lines are in the form of `key = value` which are returned in the
    file_data dictionary.

    Caller can continue reading from file_handle knowing all header lines have
    been read.
    """

    found_end = False
    for line in file_handle:
        line = line.strip()
        if line == "*** Echogram Counting ***" or line.startswith("*** Manual Marking"):
            found_end = True
            break
        tokens = line.split("=", 1)
        if len(tokens) != 2:
            tokens = line.split(":", 1)
        if len(tokens) != 2:
            if line.endswith(" ENABLED"):
                tokens = [line.replace(" ENABLED", ""), "ENABLED"]
        if len(tokens) == 2:
            key = tokens[0].strip()
            if key == "??":
                key = "Sidestream"
            value = tokens[1].strip()
            if key in file_data:
                raise Warning("key " + key + " is used twice")
            file_data[key] = value
    if not found_end:
        raise Warning("No end of header found")


def parse_body(file_handle, file_data):
    """
    Reads body lines from file_handle and puts important finds in file_data.

    Most body lines are in the form of space separated values.  Selected values
    are saved as a list of rows which are returned in the "body" key of the
    file_data dictionary.

    Caller can continue reading from file_handle knowing all body lines have
    been read.
    """

    found_end = False
    header = None
    items = None
    rows = []
    for line in file_handle:
        line = line.strip()
        if line == "*** Source File Key ***":
            found_end = True
            break
        if not header:
            header = [e.strip() for e in line.strip().split("  ") if e]
        if header:
            items = [e.strip() for e in line.strip().split(" ") if e]
        if items and len(items) >= 24:
            row = (
                items[:14]
                + [
                    items[14]
                    + items[15]
                    + items[16]
                    + items[17]
                    + items[18],  # Latitude
                    items[19] + items[20] + items[21] + items[22] + items[23],
                ]
                + items[24:]
            )
            rows.append(row)
    file_data["body_header"] = header
    file_data["body"] = rows
    # print(rows[0])
    if not found_end:
        raise Warning("No end of body found")


def parse_footer(file_handle, file_data):
    """
    Reads footer lines from file_handle and puts important finds in file_data.

    Caller can continue reading from file_handle knowing all footer lines have
    been read.
    """
    for line in file_handle:
        if line[:2] == "1:":
            line = line[2:]
        line = line.strip()
        tokens = line.split(":", 1)
        if len(tokens) == 2:
            key = tokens[0].strip()
            value = tokens[1].strip()
            file_data[key] = value


def parse_file(filename, data):
    """Reads filename and distill key elements in to the data dictionary."""

    file_data = {}
    with open(filename, "r", encoding="utf-8") as file_handle:
        try:
            parse_header(file_handle, file_data)
            parse_body(file_handle, file_data)
            parse_footer(file_handle, file_data)
        except Exception as ex:
            file_data["error"] = repr(ex)
    data[filename] = file_data


def parse_folder(root, data):
    """Get the data from all files in a folder."""

    for (foldername, _, filenames) in os.walk(root):
        for filename in filenames:
            parse_file(os.path.join(foldername, filename), data)


def print_errors(data):
    """Print any errors found in the data."""

    for file in data:
        if "error" in data[file]:
            print("{0}: {1}".format(file, data[file]["error"]))


def print_body_headers_errors(data):
    """Print any headers found in the files that do not meet expectations."""

    for file in data:
        if "body_header" in data[file]:
            header = data[file]["body_header"]
            if header not in WELL_KNOWN_HEADERS:
                print("HEADER MISMATCH: {0}: {1}".format(file, header))


def print_key_errors(data):
    """Print any unexpected keys found in the files."""

    keys = set([])
    for file in data:
        for key in data[file]:
            if key not in WELL_KNOWN_KEYS and key != "error":
                keys.add(key)
    if keys:
        print("UNKNOWN KEYS: {0}".format(keys))


def test(data):
    """Print any errors found in the file data."""

    print_errors(data)
    print_body_headers_errors(data)
    print_key_errors(data)


def fix_summary_key(old):
    """Replace space with underscore in old."""

    return old.replace("  ", "_").replace(" ", "_")


def fix_count_key(old):
    """Standardize the header names based as in FIELD_NAMES."""

    if old in FIELD_NAMES:
        return FIELD_NAMES[old]
    return old


def write_file(connection, filename, summary):
    """Save the contents of filename in database connection, return the File ID."""

    with open(filename, "rb") as file_handle:
        # open/read file as a binary blob (bytes not str)
        contents = file_handle.read()
    sql = (
        "INSERT SonarCountFiles "
        "(Processing_File_Name, Count_File_Name, Source_File_Name, Contents) "
        "OUTPUT Inserted.Sonar_Count_File_ID "
        "VALUES (?,?,?,?);"
    )
    data = [
        filename,
        summary["Count  File Name"],
        summary["Source File Name"],
        contents,
    ]
    # print(sql)
    # print(data)
    file_id = None
    try:
        with connection.cursor() as wcursor:
            file_id = wcursor.execute(sql, data).fetchval()
    except Exception as ex:
        err = "Database error:\n{0}\n{1}".format(sql, ex)
        return ("Error", err)
    if file_id is None:
        return ("Error", "Database did not return the Sonar_Count_File_ID")
    return ("Ok", file_id)


SUMMARY_KEYS = [c for c in WELL_KNOWN_KEYS[:-2] if c not in FILE_KEYS]
SUMMARY_COLUMNS = [fix_summary_key(c) for c in SUMMARY_KEYS]


def write_summary(connection, file_id, summary):
    """Save the summary dat found in file_id to the database connection."""

    columns = ",".join(SUMMARY_COLUMNS)
    values = ",".join(["?"] * len(SUMMARY_COLUMNS))
    sql = "INSERT SonarCountFileSummaries (Sonar_Count_File_ID,{0}) VALUES (?,{1});"
    sql = sql.format(columns, values)
    # print(sql)
    data = [file_id]
    for key in SUMMARY_KEYS:
        if key in summary:
            data.append(summary[key])
        else:
            data.append(None)
    try:
        with connection.cursor() as wcursor:
            # print(data)
            wcursor.execute(sql, data)
    except Exception as ex:
        err = "Database error:\n{0}\n{1}".format(sql, ex)
        print(err)
        return err
    return None


def write_counts(connection, file_id, header, counts):
    """Save the sonar counts found in file_id to the database connection."""

    columns = ",".join(header)
    for old in FIELD_NAMES:
        new = FIELD_NAMES[old]
        columns = columns.replace(old, new)
    values = ",".join(["?"] * len(header))
    sql = "INSERT SonarCountFileCounts (Sonar_Count_File_ID,{0}) VALUES (?,{1});"
    sql = sql.format(columns, values)
    # print(sql)
    try:
        with connection.cursor() as wcursor:
            for count in counts:
                data = [file_id] + count
                # print(data)
                wcursor.execute(sql, data)
    except Exception as ex:
        err = "Database error:\n{0}\n{1}".format(sql, ex)
        print(err)
        return err
    return None


def save(data, conn):
    """Save the data found in files to the database connection conn."""

    for file_name in data:
        state, response = write_file(conn, file_name, data[file_name])
        if state == "Ok":
            file_id = response
            # print(file_id)
            write_summary(conn, file_id, data[file_name])
            header = data[file_name]["body_header"]
            body = data[file_name]["body"]
            write_counts(conn, file_id, header, body)
        else:
            error_message = response
            print("Error for {0}: {1}".format(file_name, error_message))


def main(source, do_test=True, do_save=False, server=None, database=None):
    """Parses the files in source accorinding to the remaining parameters."""

    conn = None
    if do_save:
        if server is None or database is None:
            print("Error Server and Database must be specified in save mode.")
            sys.exit()
        try:
            import pyodbc
        except ImportError:
            pyodbc = None
            pydir = os.path.dirname(sys.executable)
            print("pyodbc module not found, make sure it is installed with")
            print(pydir + r"\Scripts\pip.exe install pyodbc")
            sys.exit()
        conn = get_connection_or_die(pyodbc, server, database)

    data = {}
    if os.path.isfile(source):
        parse_file(source, data)
    else:
        parse_folder(source, data)
    if do_test:
        test(data)
    if do_save:
        save(data, conn)


if __name__ == "__main__":
    main(
        CONFIG["source"],
        do_test=CONFIG["test"],
        do_save=CONFIG["save"],
        server=CONFIG["server"],
        database=CONFIG["database"],
    )
