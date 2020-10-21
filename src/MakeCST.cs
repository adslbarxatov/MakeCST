using System;
using System.Collections.Generic;
using System.IO;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс-описатель программы
	/// </summary>
	public class MakeCollisionScriptProgram
		{
		// Главная функция
		static void Main (string[] args)
			{
			// Заголовок
			Console.Title = ProgramDescription.AssemblyTitle;
			Console.Write ("\n " + ProgramDescription.AssemblyDescription + " by " + ProgramDescription.AssemblyCompany + "\n\n");

			// Проверка имени файла
			if (args.Length != 1)
				{
				Console.Write (" \x10 Usage: " +
					ProgramDescription.AssemblyMainName + " <QHullOFF_FileName" + QHullOFFReader.MasterExtension +
					">\n          or\n          " +
					ProgramDescription.AssemblyMainName + " <DFF_FileName" + DFFReader.MasterExtension + ">\n\n");
				return;
				}

			// Попытка открытия файла
			FileStream FS = null;
			try
				{
				FS = new FileStream (args[0], FileMode.Open);
				}
			catch
				{
				Console.Write (" /x13 File \"" + args[0] + "\" is unavailable\n\n");
				return;
				}

			// Определение версии файла
			bool dff = args[0].ToLower ().EndsWith (DFFReader.MasterExtension);
			List<Triangle3D> triangles = new List<Triangle3D> ();

			// Загрузка DFF
			if (dff)
				{
				BinaryReader BR = new BinaryReader (FS);
				DFFReader dffr = new DFFReader (BR);
				BR.Close ();

				if (dffr.ExtractedPoints.Count == 0)
					{
					Console.Write (" /x13 File \"" + args[0] + "\": this version is unsupported or file is empty\n\n");
					return;
					}

				for (int i = 0; i < dffr.ExtractedTriangles.Count; i++)
					{
					triangles.Add (new Triangle3D (dffr.ExtractedPoints[(int)dffr.ExtractedTriangles[i].X],
						dffr.ExtractedPoints[(int)dffr.ExtractedTriangles[i].Y],
						dffr.ExtractedPoints[(int)dffr.ExtractedTriangles[i].Z]));
					}
				}

			// Загрузка Qhull OFF
			else
				{
				StreamReader SR = new StreamReader (FS);
				QHullOFFReader qhoffr = new QHullOFFReader (SR);
				SR.Close ();

				if (qhoffr.ExtractedTriangles.Count == 0)
					{
					Console.Write (" /x13 File \"" + args[0] + "\" is unsupported or corrupted\n\n");
					return;
					}

				for (int i = 0; i < qhoffr.ExtractedTriangles.Count; i++)
					{
					triangles.Add (new Triangle3D (qhoffr.ExtractedTriangles[i]));
					}
				}

			// Чтение завершено. Сброс массива точек, формирование массива уникальных точек и ссылок на них
			FS.Close ();

			List<Point3D> points = new List<Point3D> ();
			for (int t = 0; t < triangles.Count; t++)
				{
				// Точка 1
				if (points.Contains (triangles[t].Point1))
					{
					triangles[t].Point1ArrayPosition = (uint)points.IndexOf (triangles[t].Point1);
					}
				else
					{
					triangles[t].Point1ArrayPosition = (uint)points.Count;
					points.Add (triangles[t].Point1);
					}

				// Точка 2
				if (points.Contains (triangles[t].Point2))
					{
					triangles[t].Point2ArrayPosition = (uint)points.IndexOf (triangles[t].Point2);
					}
				else
					{
					triangles[t].Point2ArrayPosition = (uint)points.Count;
					points.Add (triangles[t].Point2);
					}

				// Точка 3
				if (points.Contains (triangles[t].Point3))
					{
					triangles[t].Point3ArrayPosition = (uint)points.IndexOf (triangles[t].Point3);
					}
				else
					{
					triangles[t].Point3ArrayPosition = (uint)points.Count;
					points.Add (triangles[t].Point3);
					}
				}

			// Запись файла
			if (!CSTWriter.WriteCST (args[0], points, triangles))
				{
				Console.Write (" /x13 Cannot create file \"" + args[0] + CSTWriter.MasterExtension + "\"\n\n");
				return;
				}

			Console.Write (" /x10 Conversion completed successfully\n\n");
			}
		}
	}
