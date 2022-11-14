
# ImageToCSSGridConverter.Net

A small .Net 6 application which is able to convert RGBA images to a HTML and CSS file using the css ``display: grid`` property.

It's best use case are small images up to 256x256px.

It removes fully transparent pixels.

Some facts with the provided ``magala.png`` 6000x4000px picture:
- It's reasonable fast: Takes ~3 seconds to process (not so save to disk!)
- It kinda big: Needs about 1.4 GB of disk space
- Omnomnom Ram: Eats ~7.6GB of RAM
- I can't load the processed file in Google Chrome
- Tested on an Ryzen 3700X with 16GB 3200MHZ RAM

There is room to improve, like generating classes for colors which are used often to get the used disk space down.

The idea and provided example images are by: **[@Feuerfrosch on Twitter](https://twitter.com/Feuerfrosch_art)**

# How to use
You need Visual Studio 2022 or VS Code with .Net 6 support.

- Clone the repo
- Open ImageToCSSGridConverter.Net
- Open ImageToCSSGridConverter.Net.sln or .csproj
- Choose your image
    - Using your own image
        - Place your image in the ImageToCSSGridConverter.Net folder
        - Replace string in ``programm.cs`` with your file name
            - ``var imgName = "magala.png"``
    - Using the provided image
        - Do literally nothing
- Run the project
- Profit
    

