using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ModestTree;
using UnityEngine;
using Utils;

namespace Core.Repositories
{
    public static class TableParser
    {
        public static T ParseTable<T>(TableParseContext<T> context, string column = "Id")
        {
            context.init(context);
            foreach (var cell in context.ss.columns[column])
            {
                if (cell.value == column)
                    continue;
                
                if (cell.value.StartsWith("//"))
                    continue;
                
                if (cell.value.Trim() == string.Empty)
                    continue;

                if (cell.value == "--")
                    break;

                context.cell = cell;
                context.row = context.ss.rows[cell.value];
                context.parseRow(context);
            }
            Debug.Log($"[TableParser][ParseTable] Finished for: {context.name}");
            return context.value;
        }

        public static FloatRange ParseRange(string data)
        {
            var minMax = data.Split('/');

            return new FloatRange(Convert.ToSingle(minMax[0]), Convert.ToSingle(minMax[1]));
        }

        public static List<string> ParseStringList(string data, char sep = '/')
        {
            var result = new List<string>();
            foreach (var element in data.Split(sep))
            {
                if (!element.Trim().IsEmpty())
                {
                    result.Add(element.Trim());
                }
            }

            return result;
        }

        private static readonly Regex Whitespace = new Regex(@"\s+");

        public static string RemoveWhitespaces(string input)
        {
            return Whitespace.Replace(input, string.Empty);
        }
    }
}