using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BibliotecaDeClases.LZW
{
    public class LZW
    {
        private static string rutaDirectorioBase = Environment.CurrentDirectory;
        #region Compresion
        public static void Compresion(IFormFile Archivo, string nombre)
        {
            if (!Directory.Exists(Path.Combine(rutaDirectorioBase, "Compressions")))
            {
                Directory.CreateDirectory(Path.Combine(rutaDirectorioBase, "Compressions"));
            }
            using (var reader = new BinaryReader(Archivo.OpenReadStream()))
            {
                using (var sw = new FileStream(Path.Combine(rutaDirectorioBase, "Compressions", $"{nombre}.lzw"), FileMode.OpenOrCreate))
                {
                    using (var writer = new BinaryWriter(sw))
                    {
                        var DiccionarioLetras = new Dictionary<string, string>();
                        var bufferLength = 10000;
                        var bytebuffer = new byte[bufferLength];
                        var stringLetra = string.Empty;

                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            bytebuffer = reader.ReadBytes(bufferLength);
                            for (int i = 0; i < bytebuffer.Count(); i++)
                            {
                                stringLetra = Convert.ToString(Convert.ToChar(bytebuffer[i]));
                                if (!DiccionarioLetras.ContainsKey(stringLetra))
                                {
                                    var stringNumero = Convert.ToString(DiccionarioLetras.Count() + 1, 2);
                                    DiccionarioLetras.Add(stringLetra, stringNumero);
                                    stringLetra = string.Empty;
                                }
                            }
                        }
                        writer.Write(Encoding.UTF8.GetBytes(Convert.ToString(DiccionarioLetras.Count).PadLeft(8, '0').ToCharArray()));
                        foreach (var fila in DiccionarioLetras)
                        {
                            writer.Write(Convert.ToByte(Convert.ToChar(fila.Key[0])));
                        }
                        reader.BaseStream.Position = 0;
                        stringLetra = string.Empty;
                        var anterior = string.Empty;
                        var ListaCaracteres = new List<string>();
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            bytebuffer = reader.ReadBytes(bufferLength);
                            for (int i = 0; i < bytebuffer.Count(); i++)
                            {
                                stringLetra += Convert.ToString(Convert.ToChar(bytebuffer[i]));
                                if (!DiccionarioLetras.ContainsKey(stringLetra))
                                {
                                    var stringNumero = Convert.ToString(DiccionarioLetras.Count() + 1, 2);
                                    DiccionarioLetras.Add(stringLetra, stringNumero);
                                    ListaCaracteres.Add(DiccionarioLetras[anterior]);
                                    anterior = string.Empty;
                                    anterior += stringLetra.Last();
                                    stringLetra = anterior;
                                }
                                else
                                {
                                    anterior = stringLetra;
                                }
                            }
                        }
                        ListaCaracteres.Add(DiccionarioLetras[stringLetra]);
                        var cantidadMaximaBits = Math.Log2((float)DiccionarioLetras.Count);
                        cantidadMaximaBits = cantidadMaximaBits % 1 >= 0.5 ? Convert.ToInt32(cantidadMaximaBits) : Convert.ToInt32(cantidadMaximaBits) + 1;
                        writer.Write(Convert.ToByte(cantidadMaximaBits));

                        for (int i = 0; i < ListaCaracteres.Count; i++)
                        {
                            ListaCaracteres[i] = ListaCaracteres[i].PadLeft(Convert.ToInt32(cantidadMaximaBits), '0');
                        }
                        var BufferDeEscritura = new List<byte>();
                        var Temp = string.Empty;

                        foreach (var item in ListaCaracteres)
                        {
                            Temp += item;
                            if (Temp.Length >= 8)
                            {
                                var Max = Temp.Length / 8;
                                for (int i = 0; i < Max; i++)
                                {
                                    BufferDeEscritura.Add(Convert.ToByte(Convert.ToInt32(Temp.Substring(0, 8), 2)));
                                    Temp = Temp.Substring(8);
                                }
                            }
                        }
                        if (Temp.Length != 0)
                        {
                            BufferDeEscritura.Add(Convert.ToByte(Convert.ToInt32(Temp.PadRight(8, '0'), 2)));
                        }
                        writer.Write(BufferDeEscritura.ToArray());
                    }
                }
            }
        }
        #endregion

        #region Decompresion
        public static string Decompresion(IFormFile Archivo, string nombre)
        {
            if (!Directory.Exists(Path.Combine(rutaDirectorioBase, "Decompressions")))
            {
                Directory.CreateDirectory(Path.Combine(rutaDirectorioBase, "Decompressions"));
            }
            using (var reader = new BinaryReader(Archivo.OpenReadStream()))
            {
                using (var sw = new FileStream(Path.Combine(rutaDirectorioBase, "Decompressions", nombre), FileMode.OpenOrCreate))
                {
                    using (var writer = new BinaryWriter(sw))
                    {
                        var DiccionarioLetras = new Dictionary<int, string>();
                        var bufferLength = 10000;
                        var byteBuffer = new byte[bufferLength];
                        byteBuffer = reader.ReadBytes(8);

                        var CantidadDiccionario = Convert.ToInt32(Encoding.UTF8.GetString(byteBuffer));
                        for (int i = 0; i < CantidadDiccionario; i++)
                        {
                            byteBuffer = reader.ReadBytes(1);
                            var Letra = Convert.ToChar(byteBuffer[0]).ToString();
                            DiccionarioLetras.Add(DiccionarioLetras.Count() + 1, Letra);
                        }
                        byteBuffer = reader.ReadBytes(1);
                        var CantidadBits = Convert.ToInt32(byteBuffer[0]);
                        var AuxAnterior = string.Empty;
                        var AuxActual = string.Empty;
                        var Aux = string.Empty;
                        var Primer = true;
                        var bufferDeEscritura = new List<byte>();
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            byteBuffer = reader.ReadBytes(bufferLength);
                            foreach (var item in byteBuffer)
                            {
                                Aux += Convert.ToString(item, 2).PadLeft(8, '0');
                                while (Aux.Length >= CantidadBits)
                                {
                                    var NuevoNumero = Convert.ToInt32(Aux.Substring(0, CantidadBits), 2);
                                    if (NuevoNumero != 0)
                                    {
                                        if (Primer)
                                        {
                                            Primer = false;
                                            AuxAnterior = DiccionarioLetras[NuevoNumero];
                                            bufferDeEscritura.Add(Convert.ToByte(Convert.ToChar(AuxAnterior)));
                                        }
                                        else
                                        {
                                            if (NuevoNumero > DiccionarioLetras.Count)
                                            {
                                                AuxActual = AuxAnterior + AuxAnterior.First();
                                                DiccionarioLetras.Add(DiccionarioLetras.Count + 1, AuxActual);
                                            }
                                            else
                                            {
                                                AuxActual = DiccionarioLetras[NuevoNumero];
                                                DiccionarioLetras.Add(DiccionarioLetras.Count + 1, $"{AuxAnterior}{AuxActual.First()}");
                                            }
                                            foreach (var Letra in AuxActual)
                                            {
                                                bufferDeEscritura.Add(Convert.ToByte(Letra));
                                            }
                                            AuxAnterior = AuxActual;
                                        }
                                    }
                                    Aux = Aux.Substring(CantidadBits);
                                }
                            }
                        }
                        writer.Write(bufferDeEscritura.ToArray());
                    }
                }
            }
            return Path.Combine(rutaDirectorioBase, "Decompressions", nombre);
        }
        #endregion
    }
}
