using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;

namespace VTT
{
    [ServiceContract(CallbackContract = typeof(IServiceContractCallback))]
    public interface ISerivceContract
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
        void TileMoved(int ID, Point newPos);
        [OperationContract(IsOneWay = true)]
        void TileAdded(TileToTransfer tile);
        [OperationContract(IsOneWay = true)]
        void TileDeleted(int ID);
        [OperationContract(IsOneWay = true)]
        void CharSheetChanged(int ID, CharacterSheet cs);
        [OperationContract(IsOneWay = true)]
        void ChangeMap();
    }
    
    public interface IServiceContractCallback
    {
        [OperationContract(IsOneWay = true)]
        void ReceiveMessage(string message, string userName);
        [OperationContract(IsOneWay = true)]
        void ReceiveMap(List<TileToTransfer> map, int mapH, int mapW, int tileH, int tileW);
        [OperationContract(IsOneWay = true)]
        void ClientTileMoved(int ID, Point newPos);
        [OperationContract(IsOneWay = true)]
        void ClientTileAdded(TileToTransfer tile);
        [OperationContract(IsOneWay = true)]
        void ClientTileDeleted(int ID);
        [OperationContract(IsOneWay = true)]
        void ClientCharSheetChanged(int ID, CharacterSheet cs);
    }
}
