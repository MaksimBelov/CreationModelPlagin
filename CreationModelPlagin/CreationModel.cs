﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModelPlagin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            List<Level> levels = GetListOfLevels(doc);

            Level level1 = GetLevel(levels, "Уровень 1");
            Level level2 = GetLevel(levels, "Уровень 2");

            List<Wall> walls = new List<Wall>();

            CreateWalls(doc, 10000, 5000, level1, level2, ref walls);

            return Result.Succeeded;
        }

        public List<Level> GetListOfLevels(Document doc)
        {
            List<Level> listlevel = new FilteredElementCollector(doc)
                                        .OfClass(typeof(Level))
                                        .OfType<Level>()
                                        .ToList();
            
            if (listlevel != null)
                return listlevel;
            else
                return null;
        }

        public Level GetLevel(List<Level> levels, string levelName)
        {
            Level level = levels
                .Where(x => x.Name.Equals(levelName))
                .FirstOrDefault();

            if (level != null)
                return level;
            else
                return null;
        }

        public static void CreateWalls(Document doc, double width, double depth, Level level1, Level level2, ref List<Wall> createdWalls)
        {
            double convertredWidth = UnitUtils.ConvertToInternalUnits(width, UnitTypeId.Millimeters);
            double convertredDepth = UnitUtils.ConvertToInternalUnits(depth, UnitTypeId.Millimeters);
            double dx = convertredWidth / 2;
            double dy = convertredDepth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            Transaction transaction = new Transaction(doc, "Построение стен");
            transaction.Start();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, level1.Id, false);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
                createdWalls.Add(wall);
            }
            transaction.Commit();
        }
    }
}
