﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ExamenApp
{
    public class Pregunta
    {
        public int Id { get; set; }
        public string Enunciado { get; set; }
        public string OpcionA { get; set; }
        public string OpcionB { get; set; }
        public string OpcionC { get; set; }
        public string RespuestaCorrecta { get; set; }
        public ICollection<RespuestaExamen> Respuestas { get; set; }
    }

    public class ResultadoExamen
    {
        public int Id { get; set; }
        public string Alumno { get; set; }
        public int RespuestasCorrectas { get; set; }
        public int TotalPreguntas { get; set; }
        public double NotaFinal { get; set; }
        public ICollection<RespuestaExamen> Respuestas { get; set; }
    }

    public class RespuestaExamen
    {
        public int Id { get; set; }
        public int ResultadoExamenId { get; set; }
        public ResultadoExamen ResultadoExamen { get; set; }
        public int PreguntaId { get; set; }
        public Pregunta Pregunta { get; set; }
        public string RespuestaDada { get; set; }
        public bool EsCorrecta { get; set; }
    }

    public class ExamenDbContext : DbContext
    {
        public DbSet<Pregunta> Preguntas { get; set; }
        public DbSet<ResultadoExamen> Resultados { get; set; }
        public DbSet<RespuestaExamen> Respuestas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=examen.db");
    }

    class Program
    {
        static void Main(string[] args)
        {
            using var contexto = new ExamenDbContext();
            contexto.Database.EnsureCreated();

            while (true)
            {
                Console.WriteLine("\n--- Menú Principal ---");
                Console.WriteLine("1. Registrar pregunta");
                Console.WriteLine("2. Tomar examen");
                Console.WriteLine("3. Ver todos los resultados");
                Console.WriteLine("4. Filtrar por nombre de alumno");
                Console.WriteLine("5. Ranking de mejores alumnos");
                Console.WriteLine("6. Informe estadístico por pregunta");
                Console.WriteLine("0. Salir");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1": RegistrarPregunta(contexto); break;
                    case "2": TomarExamen(contexto); break;
                    case "3": VerResultados(contexto); break;
                    case "4": FiltrarPorAlumno(contexto); break;
                    case "5": MostrarRanking(contexto); break;
                    case "6": InformeEstadistico(contexto); break;
                    case "0": return;
                    default: Console.WriteLine("Opción inválida."); break;
                }
            }
        }

        static void RegistrarPregunta(ExamenDbContext db)
        {
            Console.Write("Enunciado: ");
            var enunciado = Console.ReadLine();
            Console.Write("Opción A: ");
            var opcionA = Console.ReadLine();
            Console.Write("Opción B: ");
            var opcionB = Console.ReadLine();
            Console.Write("Opción C: ");
            var opcionC = Console.ReadLine();
            Console.Write("Respuesta correcta (A/B/C): ");
            var correcta = Console.ReadLine().ToUpper();

            if (!new[] { "A", "B", "C" }.Contains(correcta))
            {
                Console.WriteLine("Respuesta inválida. Debe ser A, B o C.");
                return;
            }

            var pregunta = new Pregunta
            {
                Enunciado = enunciado,
                OpcionA = opcionA,
                OpcionB = opcionB,
                OpcionC = opcionC,
                RespuestaCorrecta = correcta
            };

            db.Preguntas.Add(pregunta);
            db.SaveChanges();
            Console.WriteLine("Pregunta registrada con éxito.");
        }

        static void TomarExamen(ExamenDbContext db)
        {
            Console.Write("Nombre del alumno: ");
            var nombreAlumno = Console.ReadLine();

            var totalDisponibles = db.Preguntas.Count();
            var preguntas = db.Preguntas
                .AsEnumerable()
                .OrderBy(p => Guid.NewGuid())
                .Take(Math.Min(5, totalDisponibles))
                .ToList();

            if (preguntas.Count == 0)
            {
                Console.WriteLine("No hay preguntas registradas.");
                return;
            }

            int correctas = 0;
            var resultado = new ResultadoExamen
            {
                Alumno = nombreAlumno,
                TotalPreguntas = preguntas.Count,
                Respuestas = new List<RespuestaExamen>()
            };

            foreach (var pregunta in preguntas)
            {
                Console.WriteLine($"\n{pregunta.Enunciado}");
                Console.WriteLine($"A: {pregunta.OpcionA}");
                Console.WriteLine($"B: {pregunta.OpcionB}");
                Console.WriteLine($"C: {pregunta.OpcionC}");

                string respuesta;
                do
                {
                    Console.Write("Tu respuesta (A/B/C): ");
                    respuesta = Console.ReadLine().Trim().ToUpper();
                } while (!new[] { "A", "B", "C" }.Contains(respuesta));

                bool esCorrecta = respuesta == pregunta.RespuestaCorrecta;
                if (esCorrecta) correctas++;

                resultado.Respuestas.Add(new RespuestaExamen
                {
                    PreguntaId = pregunta.Id,
                    RespuestaDada = respuesta,
                    EsCorrecta = esCorrecta
                });
            }

            resultado.RespuestasCorrectas = correctas;
            resultado.NotaFinal = (correctas / (double)preguntas.Count) * 10;

            db.Resultados.Add(resultado);
            db.SaveChanges();

            Console.WriteLine($"\nExamen finalizado. Nota: {resultado.NotaFinal:F1}/10");
        }

        static void VerResultados(ExamenDbContext db)
        {
            var resultados = db.Resultados.OrderByDescending(r => r.NotaFinal).ToList();
            foreach (var r in resultados)
            {
                Console.WriteLine($"{r.Alumno} - {r.NotaFinal:F1} puntos ({r.RespuestasCorrectas}/{r.TotalPreguntas})");
            }
        }

        static void FiltrarPorAlumno(ExamenDbContext db)
        {
            Console.Write("Nombre del alumno: ");
            var nombre = Console.ReadLine();
            var resultados = db.Resultados
                .Where(r => r.Alumno.ToLower().Contains(nombre.ToLower()))
                .OrderByDescending(r => r.NotaFinal)
                .ToList();

            if (!resultados.Any())
            {
                Console.WriteLine("No se encontraron resultados.");
                return;
            }

            foreach (var r in resultados)
            {
                Console.WriteLine($"{r.Alumno} - {r.NotaFinal:F1} ({r.RespuestasCorrectas}/{r.TotalPreguntas})");
            }
        }

        static void MostrarRanking(ExamenDbContext db)
        {
            var ranking = db.Resultados
                .GroupBy(r => r.Alumno)
                .Select(g => new
                {
                    Nombre = g.Key,
                    MejorNota = g.Max(r => r.NotaFinal)
                })
                .OrderByDescending(r => r.MejorNota)
                .ToList();

            Console.WriteLine("\n--- Ranking de Mejores Alumnos ---");
            foreach (var alumno in ranking)
            {
                Console.WriteLine($"{alumno.Nombre} - Nota Máxima: {alumno.MejorNota:F1}");
            }
        }

        static void InformeEstadistico(ExamenDbContext db)
        {
            var preguntas = db.Preguntas.Include(p => p.Respuestas).ToList();
            Console.WriteLine("\n--- Informe por Pregunta ---");

            foreach (var p in preguntas)
            {
                int total = p.Respuestas?.Count ?? 0;
                int correctas = p.Respuestas?.Count(r => r.EsCorrecta) ?? 0;
                double porcentaje = total > 0 ? (correctas / (double)total) * 100 : 0;

                Console.WriteLine($"ID {p.Id} - \"{p.Enunciado}\"");
                Console.WriteLine($"  Respondida: {total} veces");
                Console.WriteLine($"  Correctas: {correctas} ({porcentaje:F1}%)\n");
            }
        }
    }
}