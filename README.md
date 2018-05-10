# ProbabilityApp
A probability simulator, to say the least.

## Features
- Repeat a fair probability experiment with _n_ outcomes until an end condition is met.
- An end condition is qualified as an amount of repetitions, an amount of time has passed, or a pattern has happened.
- Trial specification: Repeat the experiment with that end condition _x_ times.
- Export/save data in CSV, LaTeX, HTML, and a plain text document.
- Store all files in a directory.
- Ordered storage of results. You may either specify ordered CSV, or, if HTML, plain text, or LaTeX is specified, it will append ordered results to the result document.
- CPU info is saved with trials, so you can use the program to make a benchmarking tool for your CPU.
- Multithreading, and the app will detect how many threads are in your system.

## Benchmarking Features
Run with an end condition of 60 seconds, and specify however many trials of that you desire. Longer times give more accurate results, but please note, one trial with end condition of 60 seconds takes one minute to run, so if you want to use your CPU for other things, run for a shorter time.

Using LaTeX/HTML output, you get information about average, max, min, and standard deviation of repetitions per trial and repetitions per second. For example, a Ryzen 3 1300X will get more repetitions per second than an i3-7100U.

You can test single-threaded versus multithreaded performance using this methodology and by turning down the amount of threads used by the program.

## Download
There is a built [publish.zip](https://github.com/dandalton1/ProbabilityApp/blob/master/ProbabilityApp/publish.zip?raw=true) file in ProbabilityApp/ProbabilityApp. This ZIP file contains an installer for the program; simply use the "setup.exe" file to run the installer.

## Requirements (.NET 4.6)
- .NET 4.6 is required to run the program. The system requirements that apply with installing that apply here. Please see [this Microsoft help document.](https://www.microsoft.com/en-us/download/details.aspx?id=48130)
- OS .NET 4.6 requires: Windows Vista Service Pack 2, Windows 7 Service Pack 1, Windows 8, Windows 8.1, Windows Server 2008 Service Pack 2, Windows Server 2008 R2 Service Pack 1 64-bit, Windows Server 2012 64-bit, or Windows Server 2012 R2 64-bit.
- 1 GHz or faster processor
- 512 MB of RAM
- 4.5 GB of available hard drive space for each architecture

## Program requirements
- Any x86 or x86-64 processor
- Any recent Windows OS that .NET 4.6 supports
- The program itself uses probably 20-30 MB of RAM when you're not using ordered storage. However, the program does use a large amount of memory when you store billions of ordered results (around 2-3 GB), and when limits are pushed, it will let you know in the output file(s) it creates.
- You should have ample space for what you're storing on your hard drive. Ordered results for billions of repetitions usually fall in the realm of 20-30 MB big.

## Side notes
- The program will let you know when trials complete via taskbar notifications.
- Large amounts of calculations can generate heat from your system and drain battery life on laptops/tablets.
- Opening ordered CSV files in Excel 2016 can crash the program if you include more than ten million results.
- Other programs will be relatively slow to open these files.
- LaTeX and pdftex impose a hard limit on what can compile in to a PDF.
- Don't print 1K-page documents.
