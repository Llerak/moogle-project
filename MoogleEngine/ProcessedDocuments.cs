using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
namespace MoogleEngine;
public class ProcessedDocuments
{
    //Constructor
    public ProcessedDocuments()
    {
        Stopwatch time = new Stopwatch();
        time.Start();

        this.UniversoDePalabras = new Dictionary<string, float>();
        this.Matriz = new Dictionary<string, Dictionary<string, float>>();
        AnalisisDocumentos();

        time.Stop();
        Console.WriteLine("Analisis de Datos: " + time.Elapsed.TotalMilliseconds / 1000);
    }

    //                  ATRIBUTOS
    public Dictionary<string, float> UniversoDePalabras { get; private set; }
    public Dictionary<string, Dictionary<string, float>> Matriz { get; private set; }
    private static string[] Documentos = Directory.GetFiles("../Content", "*.txt", SearchOption.AllDirectories);


    //Rellenar la Matriz Documento-Termino
    private void AnalisisDocumentos()
    {
        foreach (string Documento in Documentos)
        {
            //leer el archivo actual y obetenerlo sin signos
            string[] PalabrasDoc = ObtenerPalabras(Documento, true);

            //palabras del documento con su respectivo peso
            Dictionary<string, float> PalabrasDocTF_IDF = new Dictionary<string, float>();

            foreach (string Palabra in PalabrasDoc)
            {
                if (!this.UniversoDePalabras.ContainsKey(Palabra))
                    this.UniversoDePalabras.Add(Palabra, 0);

                if (!PalabrasDocTF_IDF.ContainsKey(Palabra))
                {
                    PalabrasDocTF_IDF.Add(Palabra, 1);
                    this.UniversoDePalabras[Palabra]++;
                }
                else
                    PalabrasDocTF_IDF[Palabra]++;
            }

            float maxValue = 0;
            foreach (float value in PalabrasDocTF_IDF.Values)
            {
                if (maxValue < value) maxValue = value;
            }

            // Computar el la frecuencia del termino TF
            foreach (string Palabra in PalabrasDocTF_IDF.Keys)
                PalabrasDocTF_IDF[Palabra] = PalabrasDocTF_IDF[Palabra] / maxValue;

            Matriz.Add(Documento, PalabrasDocTF_IDF);
        }

        //Computar el IDF de cada palabra
        foreach (string Palabra in this.UniversoDePalabras.Keys)
        {
            this.UniversoDePalabras[Palabra] = (float)(Math.Log10(Documentos.Length / this.UniversoDePalabras[Palabra]));
        }

        foreach (var Documento in Matriz.Values)
        {
            foreach (string Palabra in Documento.Keys)
                Documento[Palabra] *= UniversoDePalabras[Palabra];
        }
    }

    //devolver todos los terminos ignorando los signosde puntuacion
    public static string[] ObtenerPalabras(string Documento, bool EsDocumento = false)
    {
        List<string> ListaPalabras = new List<string>();

        if (EsDocumento)
        {
            using (FileStream DocumetoLeer = File.OpenRead(Documento))
            using (StreamReader Lector = new StreamReader(DocumetoLeer, Encoding.UTF8, true, 1024))
            {
                string Linea;
                while ((Linea = Lector.ReadLine()) != null)
                {
                    if (Linea == " " || Linea == "") continue;

                    ListaPalabras.AddRange(Proceso(Linea));
                }
            }
        }
        else ListaPalabras.AddRange(Proceso(Documento));

        return ListaPalabras.ToArray();
    }

    public static string[] Proceso(string texto)
    {
        string PalabrasLinea;
        PalabrasLinea = Regex.Replace(texto.ToLower(), "á","a")
            .Replace("é","e")
                .Replace("í","i")
                    .Replace("ó","o")
                        .Replace("ú","u")
                            .Replace("ñ","n");
        return Regex
            .Replace(PalabrasLinea,@"[^a-z0-9 ]+"," ")
            .Split(' ',StringSplitOptions.RemoveEmptyEntries);
    }
}