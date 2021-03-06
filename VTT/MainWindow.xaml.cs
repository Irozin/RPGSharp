﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.ServiceModel;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Linq;

namespace VTT
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext=false)]
    public partial class MainWindow : Window, ISerivceContract
    {
        #region variables
        private string DefaultImgFolderPath;
        //for storing folders with images and their images
        private Dictionary<string, List<BitmapImage>> imgListByDir = new Dictionary<string, List<BitmapImage>>();
        
        //for networking
        private ServiceClient serviceClient;
        private Server server;
        private ISerivceContract channel;
        //callbacks
        private List<IServiceContractCallback> clients = new List<IServiceContractCallback>();
        //for server host- ServiceHost
        private MainWindow serverType = null;

        private int TILE_HEIGHT;
        private int TILE_WIDTH;
        private int MAP_HEIGHT;
        private int MAP_WIDTH;
        //for dragging tokens
        private bool isDragging = false;
        private TokenTile dragObject;
        private Line dragLine;
        //for displaying character sheet
        private TokenTile tokenSheet;
        //list of tiles
        private List<TileToTransfer> ListOfTiles = new List<TileToTransfer>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            //feel free to change this
            DefaultImgFolderPath =
                System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "\\img";
            InitializeVariables();
            SetHostPlayerOptions();
            DisableCharacterSheet();
            InitializeCanvas();
            clients.Clear();
        }


        /// <summary>
        /// Drag window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void InitializeVariables()
        {
            MAP_HEIGHT = 0;
            MAP_WIDTH = 0;
            server = null;
            channel = null;
        }
        private void DisableCharacterSheet()
        {
            CharSheetBox.IsEnabled = false;
            CharName.Text = string.Empty;
            CharHP.Text = string.Empty;
            CharAC.Text = string.Empty;
            CharInitiative.Text = string.Empty;
        }
        private void EnableCharacterSheet()
        {
            CharSheetBox.IsEnabled = true;
        }
        private void EnableImgOptionsBox()
        {
            ImgOptionsBox.IsEnabled = true;
        }
        private void DisableImgOptionsBox()
        {
            ImgOptionsBox.IsEnabled = false;
            deleteTileCheck.IsChecked = false;
            paintTileCheck.IsChecked = false;
        }
        private void InitializeCharacterSheet()
        {
            CharSheetLayer.Items.Clear();
            CharSheetLayer.Items.Add(ImageTile.LayerModeEnum.Background);
            CharSheetLayer.Items.Add(ImageTile.LayerModeEnum.Hidden);
            CharSheetLayer.Items.Add(ImageTile.LayerModeEnum.Normal);
        }
        public void SetListOfTiles(List<TileToTransfer> map)
        {
            ListOfTiles = map;
        }

        #region graphics
        private void AddGraphicsClick(object sender, RoutedEventArgs e)
        {
            /*Two ways of loading graphics:
             *1. During startup default folder with images is loaded
             *2. We let user to load graphics by going File->Graphics->Add
             *
             * And by the way- FolderBrowserDialog() requires reference to
             * assembly System.Windows.Forms
             */
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowDialog();
            //check if given folder isn't already in library
            //if the user cancels choosing img folder dlg.SelectPath's value is ""
            if (!imgListByDir.ContainsKey(System.IO.Path.GetFileName(dlg.SelectedPath)) && dlg.SelectedPath != "")
            {
                //add folder's name to image folders tree
                imgFolderTree.Items.Add(System.IO.Path.GetFileName(dlg.SelectedPath));
                //add images from given folder
                List<BitmapImage> temp = GraphicsProvider.LoadGraphics(dlg.SelectedPath);
                imgListByDir.Add(
                    System.IO.Path.GetFileName(dlg.SelectedPath),
                    temp
                    );
            }
        }

        /// <summary>
        /// Loads all images in default folder and all images in its subfolders;
        /// Show default images' folders in imgFolderTree
        /// </summary>
        private void InitializeImgFolderTree(object sender, RoutedEventArgs e)
        {
            MenuDefaultGraphics.IsEnabled = false;
            //check if default img folder exists
            if (System.IO.Directory.Exists(DefaultImgFolderPath))
            {
                //if there are no files in the directory dir.Length's value is 0
                var dir = Directory.GetDirectories(DefaultImgFolderPath);

                for (int i = 0; i < dir.Length; ++i)
                {
                    imgFolderTree.Items.Add(System.IO.Path.GetFileName(dir[i]));
                    List<BitmapImage> temp = GraphicsProvider.LoadGraphics(dir[i]);
                    imgListByDir.Add(System.IO.Path.GetFileName(dir[i]), temp);
                }
            }
            else
            {
                MessageBox.Show("There is no default img folder");
            }
        }

        /// <summary>
        /// Lits new images in GraphicsList when image folder is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgFolderChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            //check if we have given folder's images
            if (imgListByDir.ContainsKey(imgFolderTree.SelectedItem.ToString()))
            {
                //show images in GraphicsList
                GraphicsList.Items.Clear();
                //I'm thinkig of moving it to GraphicsProvider.cs
                foreach (var image in imgListByDir[imgFolderTree.SelectedItem.ToString()])
                {
                    GraphicsList.Items.Add(new Image { Source = image });
                }
            }
        }
        #endregion

        #region ServiceMethods
        private void sendMsgButton_Click(object sender, RoutedEventArgs e)
        {
            if (chatInput.Text != string.Empty)
            {
                channel.SendMessage(DiceRoll.Roll(chatInput.Text), serviceClient.Name);
                chatInput.Clear();
                chatInput.Focus();
            }
        }

        #region IServiceContract memebers
        public void SendMessage(string message, string userName)
        {
            clients.ForEach(delegate(IServiceContractCallback c)
            {
                if (((ICommunicationObject)c).State == CommunicationState.Opened)
                {
                    c.ReceiveMessage(message, userName);
                }
                else
                {
                    clients.Remove(c);
                }
            });
        }
        
        public void Subscribe()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IServiceContractCallback>();
                if (!clients.Contains(callback))
                {
                    clients.Add(callback);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        public void SubscribePlayer()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IServiceContractCallback>();
                if (!clients.Contains(callback))
                {
                    clients.Add(callback);
                    callback.ReceiveMap(ListOfTiles, MAP_HEIGHT, MAP_WIDTH, TILE_HEIGHT, TILE_WIDTH);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        public void Unsubscribe()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IServiceContractCallback>();
                clients.Remove(callback);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        public void TileAdded(TileToTransfer tile)
        {
            ListOfTiles.Add(tile);
            clients.ForEach(delegate(IServiceContractCallback c)
            {
                if (((ICommunicationObject)c).State == CommunicationState.Opened)
                {
                    c.ClientTileAdded(tile);
                }
                else
                {
                    clients.Remove(c);
                }
            });
        }

        public void TileDeleted(int ID)
        {
            ListOfTiles.RemoveAll(x => x.ID == ID);
            clients.ForEach(delegate(IServiceContractCallback c)
            {
                if (((ICommunicationObject)c).State == CommunicationState.Opened)
                {
                    c.ClientTileDeleted(ID);
                }
                else
                {
                    clients.Remove(c);
                }
            });
        }

        public void TileMoved(int ID, Point newPos)
        {
            ListOfTiles.Find(tile => tile.ID == ID).PutPosition = newPos;
            clients.ForEach(delegate(IServiceContractCallback c)
            {
                if (((ICommunicationObject)c).State == CommunicationState.Opened)
                {
                    c.ClientTileMoved(ID, newPos);
                }
                else
                {
                    clients.Remove(c);
                }
            });
        }

        public void SendMap(List<TileToTransfer> rpgMap, int mapH, int mapW, int tileH, int tileW)
        {
            ListOfTiles = rpgMap;
            TILE_HEIGHT = tileH;
            TILE_WIDTH = tileW;
            MAP_HEIGHT = mapH;
            MAP_WIDTH = mapW;
        }
        //get map from server
        public void RequestMapToSave()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IServiceContractCallback>();
            callback.ReceiveMapToSave(ListOfTiles);
        }

        public void ChangeMap()
        {
            clients.ForEach(delegate(IServiceContractCallback c)
            {
                if (((ICommunicationObject)c).State == CommunicationState.Opened)
                {
                    c.ReceiveMap(ListOfTiles, MAP_HEIGHT, MAP_WIDTH, TILE_HEIGHT, TILE_WIDTH);
                }
                else
                {
                    clients.Remove(c);
                }
            });
        }
        #endregion
        #endregion

        #region host & join
        private void HostGame(object sender, RoutedEventArgs e)
        {
            serverType = new MainWindow();
            server = Server.GetInstance(serverType);
            serviceClient = new ServiceClient("GM", chatBox, ServiceClient.PlayerType.GameMaster, this);
            server.HostGame(serviceClient, this, chatBox);
            channel = server.GetChannel();
            if (channel != null)
                channel.SendMap(ListOfTiles, MAP_HEIGHT, MAP_WIDTH, TILE_HEIGHT, TILE_WIDTH);
            SetHostPlayerOptions();
            serviceClient.HostSetTileMapSizes(TILE_HEIGHT, TILE_WIDTH, MAP_HEIGHT, MAP_WIDTH);
        }
        
        private void StopHosting(object sender, RoutedEventArgs e)
        {
            clients.ForEach(delegate(IServiceContractCallback c)
            {
                if (((ICommunicationObject)c).State == CommunicationState.Opened)
                {
                    ((ICommunicationObject)c).Close();
                    clients.Remove(c);
                }
                else
                {
                    clients.Remove(c);
                }
            });
            server.StopHosting();
            InitializeVariables();
            SetHostPlayerOptions();
        }

        private void JoinGameSettings(object sender, RoutedEventArgs e)
        {
            JoinSettings js = new JoinSettings();
            js.Owner = this;
            js.ShowDialog();
            if (js._Join)
            {
                try
                {
                    server = Server.GetInstance(serverType);
                    serviceClient = new ServiceClient(js._Name, chatBox, ServiceClient.PlayerType.Player, this);
                    server.JoinGame(serviceClient, this, chatBox, js._Name, js._IP, js._Port);
                    channel = server.GetChannel();
                    SetHostPlayerOptions();
                }
                catch
                {
                    MessageBox.Show("Couldn't connect to the server.");
                    InitializeVariables();
                    SetHostPlayerOptions();
                }
                
            }
        }

        private void DisconnectFromServer(object sender, RoutedEventArgs e)
        {
            if (channel != null)
            {
                channel.Unsubscribe();
                server.PlayerCloseConnection();
                InitializeVariables();
                SetHostPlayerOptions();
            } 
        }

        private void SetHostPlayerOptions()
        {
            //1. No hosting/no playing
            if (server == null)
            {
                EnableImgOptionsBox();
                MenuGamemaster.IsEnabled = true;
                MenuHostGame.IsEnabled = true;
                MenuStopHosting.IsEnabled = false;
                MenuPlayer.IsEnabled = true;
                MenuJoinGame.IsEnabled = true;
                MenuDisconnect.IsEnabled = false;
                MenuMapOptions.IsEnabled = true;
                chatInput.IsEnabled = false;
                sendMsgButton.IsEnabled = false;
            }
            //2. Hosting
            else if (server.IsHosting())
            {
                EnableImgOptionsBox();
                MenuGamemaster.IsEnabled = true;
                MenuHostGame.IsEnabled = false;
                MenuStopHosting.IsEnabled = true;
                MenuPlayer.IsEnabled = false;
                MenuMapOptions.IsEnabled = true;
                chatInput.IsEnabled = true;
                sendMsgButton.IsEnabled = true;
            }
            //3. Player connected to server
            else
            {
                DisableImgOptionsBox();
                MenuGamemaster.IsEnabled = false;
                MenuPlayer.IsEnabled = true; 
                MenuJoinGame.IsEnabled = false;
                MenuDisconnect.IsEnabled = true;
                MenuMapOptions.IsEnabled = false;
                chatInput.IsEnabled = true;
                sendMsgButton.IsEnabled = true;
            }
        }
        #endregion

        #region canvas settings
        private void InitializeCanvas()
        {
            canvasScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            canvasScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            //layerModeCB.Items.Clear();
            layerModeCB.Items.Add(ImageTile.LayerModeEnum.Background);
            layerModeCB.Items.Add(ImageTile.LayerModeEnum.Hidden);
            layerModeCB.Items.Add(ImageTile.LayerModeEnum.Normal);
            InitializeCharacterSheet();
        }
        /// <summary>
        /// Draws lines on canvas that indicates tiles' sizes
        /// </summary>
        private void SetCanvas()
        {
            map.Children.Clear();
            map.Background = new SolidColorBrush(Colors.DarkGray);
            //drawing lines
            for (int x = 0; x < map.Width; x += TILE_WIDTH)
            {
                map.Children.Add(new Line()
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = map.Height,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.25
                }
                );
            }
            for (int y = 0; y < map.Height; y += TILE_HEIGHT)
            {
                map.Children.Add(new Line()
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = map.Width,
                    Y2 = y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.25
                }
                );
            }
        }
        private void CreateMapBtn(object sender, RoutedEventArgs e)
        {
            MapEditorSettings mes = new MapEditorSettings();
            mes.ShowDialog();
            if (mes._CreateMap)
            {
                CreateMap(mes._TileWidth, mes._TileHeight, mes._MapHeight, mes._MapWidth);
            }      
        }

        public void CreateMap(int tileW, int tileH, int mapH, int mapW)
        {
            TILE_HEIGHT = tileH;
            TILE_WIDTH = tileW;
            MAP_HEIGHT = mapH;
            MAP_WIDTH = mapW;
            map.Height = mapH * TILE_HEIGHT;
            map.Width = mapW * TILE_WIDTH;
            GraphicsItems.IsEnabled = true;
            ListOfTiles.Clear();
            ImageTile.ResetID();
            //InitializeCanvas();
            SetCanvas();
        }
        #endregion

        #region mapeditor
        /// <summary>
        /// Show character sheet if clicked image is a token
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void map_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            Point mousePos = Mouse.GetPosition(map);
            int mouse_X = ((int)mousePos.X / TILE_WIDTH) * TILE_WIDTH;
            int mouse_Y = ((int)mousePos.Y / TILE_HEIGHT) * TILE_HEIGHT;
            bool foundTile = false;
            //search which element was clicked
            foreach (UIElement element in map.Children)
            {
                if (element is TokenTile)
                {
                    var t = element as TokenTile;
                    if (t.PutPosition.X == mouse_X && t.PutPosition.Y == mouse_Y)
                    {
                        //check if token is hidden or in background- if so then player cannot see it's character sheet
                        if ((t.LayerMode == ImageTile.LayerModeEnum.Background.ToString() ||
                            t.LayerMode == ImageTile.LayerModeEnum.Hidden.ToString()) && 
                            server != null)
                        {
                            if (serviceClient.PT == ServiceClient.PlayerType.Player)
                            {
                                break;
                            }
                        }
                        tokenSheet = t;
                        DisplayCharSheet(t.CharSheet, t.Source);
                        foundTile = true;
                        break;
                    }
                }
            }
            if (!foundTile)
            {
                DisableCharacterSheet();
            }
            else
            {
                EnableCharacterSheet();
            }
        }
        /// <summary>
        /// Draws tile on canvas; Move tiles in the canvas; Delete tile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //check if image and layer modes are selected and if we paint
            if (GraphicsList.SelectedIndex != -1 &&
                layerModeCB.SelectedIndex != -1 &&
                paintTileCheck.IsChecked == true &&
                (tileCB.IsChecked == true || tokenCB.IsChecked == true))
            {
                AddTileOrToken();
            }
            //delete tile
            else if (deleteTileCheck.IsChecked == true)
            {
                Point mousePos = Mouse.GetPosition(map);
                int mouse_X = (int)(mousePos.X / TILE_WIDTH) * TILE_WIDTH;
                int mouse_Y = (int)(mousePos.Y / TILE_HEIGHT) * TILE_HEIGHT;
                foreach (UIElement element in map.Children)
                {
                    if (element is ImageTile)
                    {
                        var t = element as ImageTile;
                        if (t.PutPosition.X == mouse_X && t.PutPosition.Y == mouse_Y)
                        {
                            ListOfTiles.RemoveAll(tile => tile.ID == t.ID);
                            map.Children.Remove(t);
                            if (server != null)
                            {
                                channel.TileDeleted(t.ID);
                            }
                            break;
                        }
                    }
                }
            }
            //move token
            else
            {
                Point mousePos = Mouse.GetPosition(map);
                int mouse_X = (int)(mousePos.X / TILE_WIDTH) * TILE_WIDTH;
                int mouse_Y = (int)(mousePos.Y / TILE_HEIGHT) * TILE_HEIGHT;
                //search which element was clicked
                foreach (UIElement element in map.Children)
                {
                    if (element is TokenTile)
                    {
                        var t = element as TokenTile;
                        //Nobody can move tiles in background
                        if (t.PutPosition.X == mouse_X && t.PutPosition.Y == mouse_Y && 
                            t.LayerMode != ImageTile.LayerModeEnum.Background.ToString())
                        {
                            //Players can't move hidden tokens
                            if (server != null)
                            {
                                if (serviceClient.PT == ServiceClient.PlayerType.Player)
                                {
                                    if (t.LayerMode == ImageTile.LayerModeEnum.Hidden.ToString())
                                    {
                                        //break;
                                        continue;
                                    }
                                }
                            }
                            dragObject = t;
                            isDragging = true;
                            //add line that shows the distance between
                            //tile's position and mouse's position
                            dragLine = new Line();
                            dragLine.Stroke = Brushes.Blue;
                            dragLine.X1 = mouse_X + TILE_WIDTH / 2;
                            dragLine.Y1 = mouse_Y + TILE_HEIGHT / 2;
                            dragLine.X2 = dragLine.X1;
                            dragLine.Y2 = dragLine.Y1;
                            dragLine.StrokeThickness = 2;
                            map.Children.Add(dragLine);
                            break;
                        }
                    }
                }
            }
        }
        private void AddTileOrToken()
        {
            Point mousePos = Mouse.GetPosition(map);
            int mouse_X = (int)(mousePos.X / TILE_WIDTH);
            int mouse_Y = (int)(mousePos.Y / TILE_HEIGHT);

            ImageTile imgToAdd;
            TileToTransfer temp = new TileToTransfer();

            if (tileCB.IsChecked == true)
            {
                imgToAdd = new ImageTile();
                temp.CharSheet = null;
            }
            else
            {
                imgToAdd = new TokenTile();
                var TT = imgToAdd as TokenTile;
                TT.CharSheet = new CharacterSheet();
                temp.CharSheet = TT.CharSheet;
                temp.CharSheet.LayerMode = layerModeCB.SelectedItem.ToString();
            }
            //create image
            BitmapImage tempBI = new BitmapImage();
            tempBI.BeginInit();
            tempBI.StreamSource = TileToTransfer.SerializeImg(imgListByDir[imgFolderTree.SelectedItem.ToString()][GraphicsList.SelectedIndex]);
            tempBI.DecodePixelHeight = TILE_HEIGHT;
            tempBI.DecodePixelWidth = TILE_WIDTH;
            tempBI.EndInit();
            imgToAdd.Source = tempBI;
            imgToAdd.Margin = new Thickness(mouse_X * TILE_WIDTH, mouse_Y * TILE_HEIGHT, 0, 0);
            imgToAdd.Height = TILE_HEIGHT;
            imgToAdd.Width = TILE_WIDTH;
            imgToAdd.LayerMode = layerModeCB.SelectedItem.ToString();
            imgToAdd.PutPosition = new Point(mouse_X * TILE_WIDTH, mouse_Y * TILE_HEIGHT);
            imgToAdd.ID = ImageTile.AssignNextID();
            //for tranfering tiles
            temp.Source = TileToTransfer.SerializeImg(imgListByDir[imgFolderTree.SelectedItem.ToString()][GraphicsList.SelectedIndex]).GetBuffer();
            temp.LayerMode = imgToAdd.LayerMode;
            temp.PutPosition = imgToAdd.PutPosition;
            temp.ID = imgToAdd.ID;
            ListOfTiles.Add(temp);

            if (server != null)
            {
                channel.TileAdded(temp);
            }
            else
            {
                map.Children.Add(imgToAdd);
            }
            
        }
        public void AddTileOrTokenDeserialize(TileToTransfer tile)
        {
            ImageTile tileToAdd;
            if (tile.CharSheet == null)
            {
                tileToAdd = new ImageTile();
            }
            else
            {
                tileToAdd = new TokenTile();
                TokenTile temp = tileToAdd as TokenTile;
                temp.CharSheet = tile.CharSheet;
            }
            tileToAdd.PutPosition = tile.PutPosition;
            tileToAdd.Margin = new Thickness(tileToAdd.PutPosition.X, tileToAdd.PutPosition.Y, 0, 0);
            tileToAdd.Height = TILE_HEIGHT;
            tileToAdd.Width = TILE_WIDTH;
            tileToAdd.Source = tile.DeserializeImg(TILE_HEIGHT, TILE_WIDTH);
            tileToAdd.LayerMode = tile.LayerMode;
            tileToAdd.ID = tile.ID;
            map.Children.Add(tileToAdd);
        }
        /// <summary>
        /// Drag ImageTile object in the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && dragObject != null)
            {
                //update tile's position
                double pos_X = e.GetPosition(map).X - dragObject.Width;
                double pos_Y = e.GetPosition(map).Y - dragObject.Height;
                double newTop = pos_Y + (double)map.GetValue(Canvas.TopProperty);
                double newLeft = pos_X + (double)map.GetValue(Canvas.LeftProperty);
                Canvas.SetLeft(dragObject, newLeft);
                Canvas.SetTop(dragObject, newTop);
                //update the line
                dragLine.X2 = e.GetPosition(map).X;
                dragLine.Y2 = e.GetPosition(map).Y;
            }
        }
        /// <summary>
        /// Put ImageTile object in new location (after dragging)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging && dragObject != null)
            {
                //put tile in a new position
                int pos_X = (int)e.GetPosition(map).X / TILE_WIDTH;
                int pos_Y = (int)e.GetPosition(map).Y / TILE_HEIGHT;
                dragObject.Margin = new Thickness(pos_X * TILE_WIDTH, pos_Y * TILE_HEIGHT, 0, 0);
                dragObject.PutPosition = new Point(pos_X * TILE_WIDTH, pos_Y * TILE_HEIGHT);
                isDragging = false;
                //delete line
                map.Children.Remove(dragLine);
                //change tile's position value in ListOfTiles- for host only
                if (server != null)
                {
                    if (serviceClient.PT == ServiceClient.PlayerType.GameMaster)
                    {
                        ListOfTiles.Find(tile => tile.ID == dragObject.ID).PutPosition = dragObject.PutPosition;
                    }
                    channel.TileMoved(dragObject.ID, dragObject.PutPosition);
                }
                else
                {
                    ListOfTiles.Find(tile => tile.ID == dragObject.ID).PutPosition = dragObject.PutPosition;
                }
            }
        }
        private void SaveCharacterSheetBTN(object sender, RoutedEventArgs e)
        {
            CharacterSheet temp = SaveCharSheetValues();
            if (temp != null)
            {
                tokenSheet.CharSheet = temp;
                ListOfTiles.First(t => t.ID == tokenSheet.ID).CharSheet = temp;
                if (server != null)
                {
                    channel.CharSheetChanged(tokenSheet.ID, tokenSheet.CharSheet);
                }
            }
        }

        public CharacterSheet SaveCharSheetValues()
        {
            CharacterSheet cs = new CharacterSheet();
            try
            {
                cs.Name = CharName.Text;
                cs.ArmorClass = int.Parse(CharAC.Text);
                cs.HP = int.Parse(CharHP.Text);
                cs.Initiative = int.Parse(CharInitiative.Text);
                cs.LayerMode = CharSheetLayer.SelectedItem.ToString();
            }
            catch
            {
                MessageBox.Show("Cannot save character's sheet");
                cs = null;
            }
            return cs;
        }

        public void DisplayCharSheet(CharacterSheet cs, ImageSource portait)
        {
            CharPortrait.Source = portait;
            CharName.Text = cs.Name.ToString();
            CharHP.Text = cs.HP.ToString();
            CharAC.Text = cs.ArmorClass.ToString();
            CharInitiative.Text = cs.Initiative.ToString();
            CharSheetLayer.Text = cs.LayerMode;
        }

        public void CharSheetChanged(int ID, CharacterSheet cs)
        {
            clients.ForEach(delegate(IServiceContractCallback c)
            {
                if (((ICommunicationObject)c).State == CommunicationState.Opened)
                {
                    c.ClientCharSheetChanged(ID, cs);
                }
                else
                {
                    clients.Remove(c);
                }
            });
        }
        /// <summary>
        /// Request map to save from server if server is up or save map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveMap(object sender, RoutedEventArgs e)
        {
            if (server != null)
            {
                channel.RequestMapToSave();
            }
            else
            {
                MainWindow.SaveMap(ListOfTiles, TILE_HEIGHT, TILE_WIDTH, MAP_HEIGHT, MAP_WIDTH);
            }
        }
        /// <summary>
        /// Save map to file
        /// </summary>
        /// <param name="ListOfTiles"></param>
        /// <param name="TILE_HEIGHT"></param>
        /// <param name="TILE_WIDTH"></param>
        /// <param name="MAP_HEIGHT"></param>
        /// <param name="MAP_WIDTH"></param>
        public static void SaveMap(List<TileToTransfer> ListOfTiles, int TILE_HEIGHT, int TILE_WIDTH,
            int MAP_HEIGHT, int MAP_WIDTH)
        {
            if (ListOfTiles.Count > 0)
            {
                MapInfo mapInfo = new MapInfo();
                mapInfo.gameMap = ListOfTiles;
                mapInfo.tileHeight = TILE_HEIGHT;
                mapInfo.tileWidth = TILE_WIDTH;
                mapInfo.mapHeight = MAP_HEIGHT;
                mapInfo.mapWidth = MAP_WIDTH;
                MapSaveLoad.SaveMap(mapInfo);
            }
            else
            {
                MessageBox.Show("You have to make map first.");
            }
        }

        private void LoadMap(object sender, RoutedEventArgs e)
        {
            MapSaveLoad.LoadMap(this);
            if (server != null)
            {
                channel.SendMap(ListOfTiles, MAP_HEIGHT, MAP_WIDTH, TILE_HEIGHT, TILE_WIDTH);
                channel.ChangeMap();
            }
            else
            {
                foreach (var tile in ListOfTiles)
                {
                    AddTileOrTokenDeserialize(tile);
                }
            }
            ImageTile.SetID(ListOfTiles[ListOfTiles.Count - 1].ID);
        }
        //don't allow to have both paint and delete checked
        private void paintCheck_Checked(object sender, RoutedEventArgs e)
        {
            deleteTileCheck.IsChecked = false;
        }
        //don't allow to have both paint and delete checked
        private void deleteTileCheck_Checked(object sender, RoutedEventArgs e)
        {
            paintTileCheck.IsChecked = false;
        }

        private void tokenCB_Checked(object sender, RoutedEventArgs e)
        {
            tileCB.IsChecked = false;
        }

        private void tileCB_Checked(object sender, RoutedEventArgs e)
        {
            tokenCB.IsChecked = false;
        }
    }
        #endregion
}