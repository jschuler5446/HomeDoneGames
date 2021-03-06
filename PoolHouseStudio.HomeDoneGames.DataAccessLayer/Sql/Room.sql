﻿CREATE TABLE [dbo].[Room]
(
	[RoomID] INT NOT NULL IDENTITY(1,1),
    [GameTypeID] INT NOT NULL,
	[RoomCode] VARCHAR(4) NOT NULL,
	[ExpireDate] DATETIME NOT NULL,
	[CreatedDate] DATETIME NOT NULL,
	[ModifiedDate] DATETIME NOT NULL,
	CONSTRAINT UC_Room UNIQUE (RoomID, GameTypeID, RoomCode),
	CONSTRAINT PK_Room PRIMARY KEY (RoomID),
	CONSTRAINT FK_Room_GameType FOREIGN KEY (GameTypeID) REFERENCES GameType(GameTypeID)
)