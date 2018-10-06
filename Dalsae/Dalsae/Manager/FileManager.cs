﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using static Dalsae.DataManager;
using static Dalsae.Manager.AccountAgent;
using System.Windows.Media.Imaging;
using System.Net;
using System.ComponentModel;
using Dalsae.Data;
using System.Collections.Concurrent;

namespace Dalsae
{
	class FileManager
	{
		private static FileManager instence;

		private const string userFilePath = "Data/switter.ini";
		private const string optionFilePath = "Data/option.ini";
		private const string hotkeyFilePath = "Data/hotkey.ini";
		private const string followFilePath = "Data/follow.ini";
		private const string blockFilePath = "Data/block.ini";

		private const string dataFolderPath = "Data";
		private const string skinFolderPath = "Skin";
		private const string imageFolderPath = "Image";
		private const string tempFolderPath = "Temp";
		public const string soundFolderPath = "Sound";
		
		public bool isPatched { get; private set; }
		private FollowList followList = new FollowList();
		public static FileManager FileInstence { get { return GetInstence(); } }
		private static FileManager GetInstence()
		{
			if (instence == null)
			{
				instence = new Dalsae.FileManager();
				//instence.Init();
			}

			return instence;
		}

		public void Init()
		{
			CheckDataFolder();
			DeletePatchFile();
			//DeleteConfigFile();
			//LoadToken();
			LoadOption();
			LoadHotKey();
			LoadFollowList();
			LoadBlockList();
			LoadSkin();
			Manager.AccountAgent.accountInstence.OnChangeAccount += OnChangeAccount;
			//LoadSkin();
		}

		private void OnChangeAccount(UserKey switter)
		{
			LoadFollowList();
			LoadBlockList();
		}

		private void CheckDataFolder()
		{
			if (Directory.Exists(dataFolderPath) == false)
				System.IO.Directory.CreateDirectory(dataFolderPath);
			if (Directory.Exists(soundFolderPath) == false)
				System.IO.Directory.CreateDirectory(soundFolderPath);
			if (Directory.Exists(skinFolderPath) == false)
				System.IO.Directory.CreateDirectory(skinFolderPath);
			if (Directory.Exists(imageFolderPath) == false)
				System.IO.Directory.CreateDirectory(imageFolderPath);
			if (Directory.Exists(tempFolderPath) == false)
				System.IO.Directory.CreateDirectory(tempFolderPath);
		}

		private void DeletePatchFile()
		{
			isPatched = File.Exists("Dalsae_Patch.exe");
			for (int i = 0; i < 5; i++)
			{
				try
				{
					File.Delete("Dalsae_Patch.exe");
					break;
				}
				catch (Exception e)
				{
					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		private void DeleteConfigFile()
		{
			for (int i = 0; i < 5; i++)
			{
				try
				{
					File.Delete("Dalsae.exe.config");
					break;
				}
				catch (Exception e)
				{
					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		private void LoadBlockList()
		{
			BlockList blockList = null;
			//Dictionary<long, char> dicBlock = null;
			if (File.Exists(blockFilePath) && DataInstence.option.isLoadBlock == false)
			{
				string json = string.Empty;
				using (StreamReader sr = new StreamReader(blockFilePath))
				{
					json = sr.ReadToEnd();
				}
				try
				{ blockList = JsonConvert.DeserializeObject<BlockList>(json); }
				catch(Exception e)
				{ App.SendException("Load BlockList Error",""); }
				//dicBlock = JsonConvert.DeserializeObject<Dictionary<long, char>>(json);
			}
			if (blockList == null)
				blockList = new BlockList();
			DataInstence.blockList = blockList;
		}

		private void LoadFollowList()
		{
			if (File.Exists(followFilePath) && DataInstence.option.isLoadFollwing == false)
			{
				string json = string.Empty;
				using (StreamReader sr = new StreamReader(followFilePath))
				{
					json = sr.ReadToEnd();
				}
				try
				{ followList = JsonConvert.DeserializeObject<FollowList>(json); }
				catch(Exception e){ App.SendException("Load Follow Error", ""); }
				if (accountInstence.selectedAccount != null)
					DataInstence.dicFollwing = followList.dicFollow[accountInstence.selectedAccount.id];
			}
			else
			{
				followList = new FollowList();
			}
		}

		private void LoadSkin()
		{
			Skin skin = null;
			string path = skinFolderPath + "/" + DataInstence.option.skinName + ".txt";
			if (File.Exists(path))//스킨 파일 체크
			{
				string json = string.Empty;

				using (StreamReader sr = new StreamReader(path))
				{
					json = sr.ReadToEnd();
				}

				try { skin = JsonConvert.DeserializeObject<Skin>(json); }
				catch { skin = new Skin(); }
			}
			else
			{
				skin = new Skin();
			}
			DataInstence.UpdateSkin(skin);
		}

		private void LoadOption()
		{
			Option option = null;
			if (File.Exists(optionFilePath))
			{
				string json = string.Empty;

				using (StreamReader sr = new StreamReader(optionFilePath))
				{
					json = sr.ReadToEnd();
				}
				if (json.Length < 10)
				{
					App.SendException("option load error, option.json is empty", "jsonnnnnn");
				}
				try { option = JsonConvert.DeserializeObject<Option>(json); }
				catch (Exception e) { App.SendException("Load Option Error", ""); }
				if (option == null) option = new Option();

				//DataInstence.option = option;
				DataInstence.SetOption(option);
			}
			else
			{
				option = new Option();
				DataInstence.SetOption(option);
			}
		}

		private void LoadHotKey()
		{
			HotKeys hotkey = null;
			if (System.IO.File.Exists(hotkeyFilePath))
			{
				string json = string.Empty;

				using (StreamReader sr = new StreamReader(hotkeyFilePath))
				{
					json = sr.ReadToEnd();
				}
				try { hotkey = JsonConvert.DeserializeObject<HotKeys>(json); }
				catch (Exception e) { App.SendException("Load hotky Error", ""); }
			}
			if (hotkey == null)
			{
				hotkey = new HotKeys();
				UpdateHotkey(hotkey);
			}
			DataInstence.UpdateHotkeys(hotkey);
		}

		public Switter LoadSwitter()
		{
			Switter switter = null;
			if (File.Exists(userFilePath))
			{
				string json = string.Empty;

				using (StreamReader sr = new StreamReader(userFilePath))
				{
					json = sr.ReadToEnd();
				}

				try { switter = JsonConvert.DeserializeObject<Switter>(json); }
				catch (Exception e) { App.SendException("Load Switter Error", ""); }
			}
			if (switter == null)
				switter = new Switter();

			return switter;
		}
		
		
		public string[] GetSoundList()
		{
			if (Directory.Exists(soundFolderPath))
			{
				DirectoryInfo di = new DirectoryInfo(soundFolderPath);
				FileInfo[] fi = di.GetFiles();
				List<string> listSound = new List<string>();
				for (int i = 0; i < fi.Length; i++)
				{
					if (fi[i].Extension == ".wav")//wav일 경우에만 추가
						listSound.Add(fi[i].Name);
				}

				return listSound.ToArray();
			}
			else
			{
				return null;
			}
		}

		public string[] GetSkinList()
		{
			if (Directory.Exists(skinFolderPath))
			{
				DirectoryInfo di = new DirectoryInfo(skinFolderPath);
				FileInfo[] fi = di.GetFiles();
				List<string> listSkin = new List<string>();
				for (int i = 0; i < fi.Length; i++)
				{
					if (fi[i].Extension == ".txt")//txt일경우에만 추가
						listSkin.Add(fi[i].Name.Replace(".txt", ""));
				}

				return listSkin.ToArray();
			}
			else
			{
				return null;
			}
		}

		public void UpdateSkin()
		{
			LoadSkin();
		}

		public void TestStream(string json)
		{
			File.AppendAllText("Data/Stream.txt", json);
			File.AppendAllText("Data/Stream.txt", "\n\n");
		}

		public void SaveSwitter(Switter switter)
		{
			if (switter.dicUserKey.ContainsKey(0))
				switter.dicUserKey.Remove(0);
			string json = JsonConvert.SerializeObject(switter);
			if (string.IsNullOrEmpty(json)) return;


			using (FileStream fs = new FileStream(userFilePath, FileMode.Create))
			using (StreamWriter writer = new StreamWriter(fs))
			{
				writer.Write(json);
				writer.Flush();
			}
		}
		
		public bool ExistsVideo(string name)
		{
			return File.Exists($"{DataInstence.option.imageFolderPath}/{name}");
		}

		public string GetVideoFilePath(string fileName)
		{
			return $@"{tempFolderPath}/{fileName}.mp4";
		}


		public void DownloadVideo(string http, string fileName, DownloadProgressChangedEventHandler downChanged, 
											AsyncCompletedEventHandler downComplate)
		{
			CheckCountAndDeleteVideo();//오래된 파일 삭제 후 다운로드

			using (WebClient web = new WebClient())
			{
				web.DownloadFileAsync(new Uri(http), $"{tempFolderPath}/{fileName}.mp4");
				web.DownloadProgressChanged += downChanged;
				web.DownloadFileCompleted += downComplate;
			}
		}

		/// <summary>
		/// 동영상 임시 파일 개수 체크 후 오래된 파일 삭제
		/// </summary>
		private void CheckCountAndDeleteVideo()
		{
			string []arrFiles = Directory.GetFiles(tempFolderPath);

			if (arrFiles.Length < 40) return;

			List<FileInfo> listFile = new List<FileInfo>();
			for (int i = 0; i < arrFiles.Length; i++)
				listFile.Add(new FileInfo(arrFiles[i]));

			listFile.Sort(Compare);
			for (int i = 0; i < listFile.Count - 5; i++)
				listFile[i].Delete();
		}

		private int Compare(FileInfo x, FileInfo y)
		{
			return x.LastWriteTime.CompareTo(y.LastWriteTime);
		}

		

		public void UpdateBlockList(BlockList blockList)
		{
			if (blockList == null) return;
			string json = JsonConvert.SerializeObject(blockList);
			if (string.IsNullOrEmpty(json)) return;

			using (FileStream fs = new FileStream(blockFilePath, FileMode.Create))
			using (StreamWriter writer = new StreamWriter(fs))
			{
				writer.Write(json);
			}
			//File.WriteAllText(blockFilePath, json);
		}

		public void UpdateHotkey(HotKeys hotkey)
		{
			if (hotkey == null) return;
			string json = JsonConvert.SerializeObject(hotkey);
			if (string.IsNullOrEmpty(json)) return;

			using (FileStream fs = new FileStream(hotkeyFilePath, FileMode.Create))
			using (StreamWriter writer = new StreamWriter(fs))
			{
				writer.Write(json);
				writer.Flush();
			}
			//File.WriteAllText(hotkeyFilePath, json);
		}

		public void UpdateOption(Option option)
		{
			if (option == null) return;
			string json = JsonConvert.SerializeObject(option);
			if (string.IsNullOrEmpty(json)) return;

			using (FileStream fs = new FileStream(optionFilePath, FileMode.Create))
			using (StreamWriter writer = new StreamWriter(fs))
			{
				writer.Write(json);
				writer.Flush();
			}
		}

		public void SaveImage(string url, BitmapImage bitmapImage)
		{
			string path = DataInstence.option.imageFolderPath;
			if (Directory.Exists(path) == false)
			{
				if (Directory.Exists(imageFolderPath) == false)
					Directory.CreateDirectory(imageFolderPath);
				path = imageFolderPath;
				DataInstence.option.imageFolderPath = path;
			}
			string http = url;
			string fileName = http.Replace("https://pbs.twimg.com/media/", "");
			string ext = fileName.Substring(fileName.IndexOf('.') + 1, 3);
			using (MemoryStream outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(bitmapImage));
				enc.Save(outStream);
                using (var bitmap = new Bitmap(outStream))
                {
                    if (ext == "jpg")
                        bitmap.Save($"{path}/{fileName}", ImageFormat.Jpeg);
                    else if (ext == "png")
                        bitmap.Save($"{path}/{fileName}", ImageFormat.Png);
                }
			}			
		}
	}
}
