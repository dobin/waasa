# Windows App Attack Surface Analyzer

It has three main features:
* Display all file extensions and their associated programs from a windows machine
* Test a content filter whitelist/blacklist against the file extensions
* Give information about which file extensions are malicious

This can give a RedTeamer: 
* An overview of the attack surface of a machine
* A list of files or ways the content filter can be bypassed

There is a online version: 
* [badfiles.ch](https://badfiles.ch): A list of malicious file extensions
* [badfiles.ch/windows.html](https://badfiles.ch/windows.html): Windows attack surface


# Usage

## Attack Surface

![Waasa Attack Surface Windows](https://raw.githubusercontent.com/dobin/waasa/master/doc/waasa-win.png)


## Content Filter Test

![Waasa Content Filter Examine](https://raw.githubusercontent.com/dobin/waasa/master/doc/waasa-contentfilter-examine.png)

![Waasa Content Filter File](https://raw.githubusercontent.com/dobin/waasa/master/doc/waasa-contentfilter-file.png)


## Usage Console (beta)

* It uses `./waasa.json` as default dump filename by default
* You can copy `waasa.json` to another machine for analysis


Create a registry dump (to `waasa.json`):
```
> .\waasa.exe dump
```

Create CSV from dump:
```
> .\waasa.exe dump --csv output.csv
```

Create all files in `./output/`:
```
> .\waasa.exe --files
```


## Files 

* gathered_data.json: a dump from the registry of a machine and more, around 10MB
* waasa.json:  parsed registry dump (from gathered_data.json)
* waasa.csv: Output to CSV
* waasa.txt: Input of file extensions


## Example Results

From a fresh Windows 10 VM with Visual Studio installed:

* [Result CSV File](https://github.com/dobin/waasa/blob/master/data/windev.csv)
* [Dump File Download](https://raw.githubusercontent.com/dobin/waasa/master/data/windev.json)


## Notes about the results

Windows basically knows three types of actions when clicking a file: 
1) Execute the associated program
2) Show "Open With" dialog, where a program is preselected (recommended)
3) Show "Open With" dialog, no preselection or recommendation

Because of Windows restrictions, waasa will treat 1) and 2) mostly as the same. 
Which makes sense from an attackers perspective too, as users will likely click "Open With Recommended"
entry. 

The results are mostly based on Windows `shlwap` interface, which gives a lot of wrong results. 
I tried to improve the algorihmn, but there are still misidentifications possible. Double check
your results (by manually clicking on the files). 

![OpenWith 1](https://raw.githubusercontent.com/dobin/waasa/master/doc/openwith-1.png)
![Recommended](https://raw.githubusercontent.com/dobin/waasa/master/doc/recommended-1.png)

