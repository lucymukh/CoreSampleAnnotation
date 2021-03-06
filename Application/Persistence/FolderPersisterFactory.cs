﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoreSampleAnnotation.Persistence
{
    class FolderPersisterFactory : IProjectPersisterFactory
    {
        readonly private string imagesDirectory = "Photos";
        readonly private string layersTemplateDirectory = "LayersTemplate";
        readonly private string layersRankFile = "LayerRanks.csv";

        public bool TryCreateNew(out IProjectPersister persister)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Выберите папку для нового проекта";

            bool done = false;

            while (!done)
            {
                CommonFileDialogResult result = dialog.ShowDialog();
                switch (result)
                {
                    case CommonFileDialogResult.Ok:

                        string path = dialog.FileName;
                        if (!System.IO.Directory.Exists(path))
                            System.IO.Directory.CreateDirectory(path);
                        else
                        {
                            var files = System.IO.Directory.GetFiles(path);
                            var dirs = System.IO.Directory.GetDirectories(path);
                            if (files.Length + dirs.Length > 0)
                            {
                                MessageBox.Show("Выбранная папка не пуста. Выберите пустую папку для нового проекта", "Папка не пуста", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                continue;
                            }                            
                        }
                        persister = new FolderPersister(path);
                        var imageStorage = new FolderImageStorage(System.IO.Path.Combine(path, imagesDirectory));

                        //coping default layer template
                        string exeLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                        string defaultTemplatePath = Path.Combine(exeLocation, "DefaultLayersTemplate");
                        PathUtils.DeepCopy(defaultTemplatePath, Path.Combine(path, layersTemplateDirectory));
                        var layersTemplate = new FolderLayersTemplateSource(Path.Combine(path, layersTemplateDirectory));

                        //copying layer ranks
                        File.Copy(Path.Combine(exeLocation, "DefaultLayerRanks.csv"), Path.Combine(path, layersRankFile));
                        var layerRanks = new CsvFileLayerRankSource(Path.Combine(path, layersRankFile));

                        //copying default clumn settings
                        Directory.CreateDirectory(Path.Combine(path,JsonFileColumnSettings.TamplatesDir));
                        File.Copy(
                            Path.Combine(exeLocation, "DefaultColumnSettings.json"),
                            Path.Combine(path, JsonFileColumnSettings.TamplatesDir,JsonFileColumnSettings.DefaultSettingsFile));

                        var colSettingsPersister = new JsonFileColumnSettings();
                        Directory.SetCurrentDirectory(path); //after this the paths in project will be relative

                        persister.SaveProject(
                            new ProjectVM(imageStorage, layersTemplate, layerRanks, colSettingsPersister)); //new clean project is dumped to disk
                        
                        return true;
                    default:
                        done = true;
                        break;
                }
            }
            persister = null;            
            return false;
        }

        public bool TryRestoreExisting(out IProjectPersister persister)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Выберите папку сохраненного ранее проекта";

            CommonFileDialogResult result = dialog.ShowDialog();
            switch (result)
            {
                case CommonFileDialogResult.Ok:

                    string path = dialog.FileName;
                    
                    persister = new FolderPersister(path);

                    System.IO.Directory.SetCurrentDirectory(path);

                    return true;
                default:
                    persister = null;
                    return false;
            }
        }
    }
}
