﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;

namespace VTT
{
    [ServiceContract(CallbackContract = typeof(IChatCallback))]
    public interface IChat
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(string message, string userName);
        [OperationContract(IsOneWay = true)]
        void Subscribe();
        [OperationContract(IsOneWay = true)]
        void Unsubscribe();
        [OperationContract(IsOneWay = true)]
        void SubscribePlayer();
        [OperationContract(IsOneWay = true)]
        void SendMap(List<TileToTransfer> rpgMap, int mapH, int mapW, int tileH, int tileW);
        [OperationContract(IsOneWay = true)]
        void TileMoved(TileToTransfer tile);
        [OperationContract(IsOneWay = true)]
        void TileAdded(TileToTransfer tile);
        [OperationContract(IsOneWay = true)]
        void TileDeleted(int ID); //TileToTransfer tile
    }
    
    public interface IChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void ReceiveMessage(string message, string userName);
        [OperationContract(IsOneWay = true)]
        void ReceiveMap(List<TileToTransfer> map, int mapH, int mapW, int tileH, int tileW);
        [OperationContract(IsOneWay = true)]
        void ClientTileMoved(TileToTransfer tile);
        [OperationContract(IsOneWay = true)]
        void ClientTileAdded(TileToTransfer tile);
        [OperationContract(IsOneWay = true)]
        void ClientTileDeleted(int ID); //TileToTransfer tile
    }
}
