using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var stopWatch = Stopwatch.StartNew();

        var imgName = "magala.png";
        Bitmap bmp = Bitmap.FromFile(imgName) as Bitmap;
        if (bmp == null) Environment.Exit(1);

        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
        var depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8; //bytes per pixel

        var pixels = new byte[data.Width * data.Height * depth];

        //copy pixels to buffer
        Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                
        var cores = Environment.ProcessorCount;
        var rowsPerThread = bmp.Height / cores;
        var lostPixel = bmp.Height - (rowsPerThread * cores);

        var tasks = new Task<string>[cores];

        for(int i = 0; i <= cores - 1; i++) // Create a thread for every core with height/core rows
        {
            var to = (rowsPerThread * (i + 1));
            if(i + 1 == cores) 
                to += lostPixel;
            var width = data.Width;
            var startPoint = rowsPerThread * i;

            Debug.Assert(startPoint != to);
            var core = i;
            Console.WriteLine($"Adding T{i} with {to - startPoint} rows");
            tasks[i] = Task.Run(() => getDivForPixels(pixels, startPoint, to, width, depth, core));
        }

        var tmp = string.Empty;
        for (;;)
        {
            if (tasks.All(x => x.Status == TaskStatus.RanToCompletion))
                break;

            var ouput = $"Running {tasks.Where(x => x.Status != TaskStatus.RanToCompletion).Count()} Threads\n";
            for (var t = 0; t < tasks.Length; t++)
                ouput += $"- T{t} - Status: {tasks[t].Status}\n";

            if(tmp != ouput)
            {
                Console.Clear();
                Console.WriteLine(ouput);
            }

            Task.Delay(1000);
        }

        pixels = null;

        var divContainer = new List<String>();
        foreach (var t in tasks)
        {
            divContainer.Add(t.Result);
            t.Dispose();
            GC.Collect();
        }

        bmp.UnlockBits(data);

        Console.WriteLine("Writing stylesheet");
        createStyleFile(bmp.Size, imgName);

        Console.WriteLine("Writing html file");
        createImageFile(divContainer, imgName);

        Console.WriteLine("Finished!");
        stopWatch.Stop();
        Console.WriteLine($"Programm finished {bmp.Height * bmp.Width} pixel in {stopWatch.Elapsed}");
    }

    private static Task<string> getDivForPixels(byte[] pixels, int fromH, int toH, int width, int depth, int threadId)
    {
        Debug.WriteLine($"Creating thread {threadId} from {fromH} to {toH}");
        return Task.Run(() => {
        var stopWatch = Stopwatch.StartNew();
        var str = new StringBuilder();
        for(int h = fromH; h < toH; h++)
        {
            for(int w = 0; w < width; w++)
            {
                var offset = ((h * width) + w) * depth; // Calculate the pixel data offset in the array

                var a = pixels[offset + 3];
                if (a == 0) continue; // Skip transparent pixel

                var b = pixels[offset];
                var g = pixels[offset + 1];
                var r = pixels[offset + 2];

                // https://stackoverflow.com/a/38493495
                str.Append($"<a style=\"background:#{r:X2}{g:X2}{b:X2}{a:X2};grid-column:{w + 1};grid-row:{h + 1}\"/>");
                }
            }

        stopWatch.Stop();
        Debug.WriteLine($"Thread {threadId} finished {(toH - fromH) * width} pixel in {stopWatch.Elapsed} Seconds");
        return str.ToString();
        });        
    }

    private static void createStyleFile(Size size, string imgName)
    {
        var name = "style.css";
        var sb = new StringBuilder();
        sb.Append(getTemplate(name));
        sb.Replace("%REPLACECOL%", (size.Width + 1).ToString());
        sb.Replace("%REPLACEROW%", (size.Height + 1).ToString());
        sb.Replace("%REPLACEWIDTH%", size.Width.ToString());
        sb.Replace("%REPLACEHEIGHT%", size.Height.ToString());
        writeTemplate($"{imgName}.css", sb.ToString());
    }

    private static void createImageFile(List<string> content, string imgName)
    {
        var name = "templateStart.html";
        var end = getTemplate("templateEnd.html");
        var sb = new StringBuilder();
        sb.Append(getTemplate(name));
        sb.Replace("%REPLACECSS%", imgName);
        sb.Replace("%REPLACENAME%", imgName);

        content.Insert(0, sb.ToString());
        content.Add(end);
        SaveFile("build/" + $"{imgName}.html", content.ToArray());
    }

    private static void writeTemplate(string name, string content)
    {
        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "build"));
        File.WriteAllText("build/" + name, content);        
    }

    private static string getTemplate(string name)
    {
        return File.ReadAllText(name);
    }

    // https://social.msdn.microsoft.com/Forums/vstudio/en-US/29fc1cbe-b0c7-44b7-b1ee-d09a8c07226a/unable-to-write-a-large-text-file?forum=csharpgeneral#:~:text=I%27ve%20just%20tested,the%20example%20posted)
    private static void SaveFile(string filename, string[] content)
    {
        if (File.Exists(filename)) File.Delete(filename);
        using (var file = new StreamWriter(filename))
        {            
            foreach(var item in content)
            {
                file.WriteLine(item);
            }
        }
    }
}