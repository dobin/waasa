# Windows App Attack Surface Analyzer

Shows which file extensions gets opened by which program (executables).


## Usage GUI

Double click `waasa.exe`.

![Waasa GUI](https://raw.githubusercontent.com/dobin/waasa/master/doc/gui.png)


## Usage Console

* It uses `./waasa.json` as default dump filename by default
* You can copy `waasa.json` to another machine for analysis


Create a dump (optional):
```
> .\waasa.exe --dump
```

Create CSV from dump:
```
> .\waasa.exe --csv output.csv
```

Create all files in `./output/`:
```
> .\waasa.exe --files
```



## Files 

* waasa.json: a dump from the registry of a machine, and additional information. Main working file.
* waasa.csv: Output to CSV
* waasa.txt: Input of file extensions
* gathered_data.json: Registry dump

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

