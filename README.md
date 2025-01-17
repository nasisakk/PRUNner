# PRUNner
PRUNner is a cross-platform standalone tool supposed to ease up base planning on an empire-wide scale by replacing spreadsheets with a custom-made application, yielding way higher response times than one could ever achieve in Google Sheets.

You can grab the latest version over at our [Releases](https://github.com/Jacudibu/PRUNner/releases): there's a .zip file for every major platform, just unpack and run the PRUNner file inside.

### Screenshots
First, let's find a planet to settle!
![PlanetFinder](https://user-images.githubusercontent.com/9059719/125678028-648e6575-e968-4440-9f01-e918028c9174.png)

Then, plan out your base!
![BasePlanner](https://user-images.githubusercontent.com/9059719/122645029-6773e180-d118-11eb-89be-6dfe085d4ae9.png)

And finally, have an overview over your entire empire.
![EmpireOverview](https://user-images.githubusercontent.com/9059719/121958208-b78f2480-cd63-11eb-953c-c6537b079cd3.png)

### Contributing
I'm open to any kind of feedback or suggestions. This is my second Application using WPF / Avalonia, so there's probably a lot of stuff that could be done better.

Check out our [Issues](https://github.com/Jacudibu/PRUNner/issues) for a list of things that are planned (or have been suggested). If you want to help implementing any of them or add something else, it's probably best to join the [PrUn Community Tools Discord Server](https://discord.gg/2MDR5DYSfY) and/or creating an issue first.

### Building / Running PRUNner from Code
You'll need the .Net 5 SDK installed to build the application from source.
There's no special configuration required. If you want to see how to build the app via the command line for your operating system, take a look at [publish.sh](https://github.com/Jacudibu/PRUNner/blob/main/publish.sh).

I'd recommend grabbing the FIOCache folder from one of the releases and putting it into the output directory, as downloading all of it can take a while.