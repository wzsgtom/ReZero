﻿using ReZero.TextTemplate;
using System.Text;

/****************该功能还在开发中*****************/
var x = new TextTemplateManager();
var template = @"
            <div v-if=""condition"">Visible</div>
            <div v-for=""item in collection"">{{item}}</div>
        ";
var data = new { condition = true, collection = new[] { "Item 1", "Item 2", "Item 3" } };
var output = new StringBuilder();
var str=x.RenderTemplate(template, data);
Console.WriteLine(str);
Console.WriteLine("该功能还在开发中!!");
Console.ReadLine();