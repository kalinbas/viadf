USE [viadf]
GO

CREATE TABLE [dbo].[Colonia](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[SourceID] [int] NOT NULL,
	[DelegacionID] [int] NOT NULL,
	[SeoName] [varchar](255) NOT NULL,
 CONSTRAINT [PK_Colonia] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[Delegacion](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[SourceID] [int] NOT NULL,
	[EstadoID] [int] NOT NULL,
	[SeoName] [varchar](255) NOT NULL,
 CONSTRAINT [PK_Delegacion] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[Estado](
	[ID] [int] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[SourceID] [int] NOT NULL,
	[SeoName] [varchar](255) NOT NULL,
 CONSTRAINT [PK_Estado] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE TABLE [dbo].[Mail](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Subject] [varchar](255) NULL,
	[Name] [varchar](255) NULL,
	[Email] [varchar](255) NULL,
	[Message] [varchar](max) NULL,
 CONSTRAINT [PK_Message] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [dbo].[POI](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[Lat] [float] NOT NULL,
	[Lng] [float] NOT NULL,
	[StreetCrossingID] [int] NULL,
 CONSTRAINT [PK_POI] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[Route](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[Status] [int] NOT NULL,
	[SplitRoutePieceID] [int] NULL,
	[FromName] [varchar](255) NULL,
	[ToName] [varchar](255) NULL,
	[TypeID] [int] NOT NULL,
	[Description] [varchar](max) NULL,
	[Email] [varchar](255) NULL,
	[AverageSpeed] [float] NULL,
	[PriceTypeID] [int] NULL,
	[PriceDefinition] [varchar](50) NULL,
	[Frequency] [float] NULL,
	[SeoName] [varchar](255) NOT NULL,
	[ParentRouteID] [int] NULL,
 CONSTRAINT [PK_Route] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [dbo].[RoutePiece](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[Lat] [float] NOT NULL,
	[Lng] [float] NOT NULL,
	[RouteID] [int] NOT NULL,
	[StreetCrossingID] [int] NULL,
	[SeoName] [varchar](255) NULL,
 CONSTRAINT [PK_RoutePiece] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[SearchHistory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IpAdress] [varchar](20) NOT NULL,
	[FromName] [varchar](255) NULL,
	[FromLat] [float] NOT NULL,
	[FromLng] [float] NOT NULL,
	[ToName] [varchar](255) NULL,
	[ToLat] [float] NOT NULL,
	[ToLng] [float] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SearchHistory] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[SearchIndex](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RoutePieceID] [int] NOT NULL,
	[RoutePiece2ID] [int] NOT NULL,
	[Cost] [float] NOT NULL,
 CONSTRAINT [PK_SearchIndex] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[Street](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[SourceID] [int] NOT NULL,
	[ColoniaID] [int] NOT NULL,
	[FullName] [varchar](120) NOT NULL,
 CONSTRAINT [PK_Street] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[StreetCrossing](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[SourceID] [int] NOT NULL,
	[StreetID] [int] NOT NULL,
	[Street2ID] [int] NOT NULL,
	[Lat] [float] NOT NULL,
	[Lng] [float] NOT NULL,
 CONSTRAINT [PK_StreetCrossing] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[SystemException](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[Name] [varchar](max) NOT NULL,
	[StackTrace] [varchar](max) NOT NULL,
	[IpAdress] [varchar](20) NULL,
 CONSTRAINT [PK_SystemException] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [dbo].[Type](
	[ID] [int] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[AverageSpeed] [float] NOT NULL,
	[HasNamedStationList] [bit] NOT NULL,
	[ShowInWeb] [bit] NOT NULL,
	[PriceTypeID] [int] NOT NULL,
	[PriceDefinition] [varchar](50) NULL,
	[Frequency] [float] NOT NULL,
	[IsWalkingType] [bit] NOT NULL,
	[SeoName] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Type] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX [IX_Street] ON [dbo].[Street]
(
	[FullName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_StreetCrossing] ON [dbo].[StreetCrossing]
(
	[StreetID] ASC,
	[Street2ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_StreetCrossing_Lat] ON [dbo].[StreetCrossing]
(
	[Lat] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_StreetCrossing_Lng] ON [dbo].[StreetCrossing]
(
	[Lng] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Colonia]  WITH CHECK ADD  CONSTRAINT [FK_Colonia_Delegacion] FOREIGN KEY([DelegacionID])
REFERENCES [dbo].[Delegacion] ([ID])
GO
ALTER TABLE [dbo].[Colonia] CHECK CONSTRAINT [FK_Colonia_Delegacion]
GO
ALTER TABLE [dbo].[Delegacion]  WITH CHECK ADD  CONSTRAINT [FK_Delegacion_Estado] FOREIGN KEY([EstadoID])
REFERENCES [dbo].[Estado] ([ID])
GO
ALTER TABLE [dbo].[Delegacion] CHECK CONSTRAINT [FK_Delegacion_Estado]
GO
ALTER TABLE [dbo].[POI]  WITH CHECK ADD  CONSTRAINT [FK_POI_StreetCrossing] FOREIGN KEY([StreetCrossingID])
REFERENCES [dbo].[StreetCrossing] ([ID])
GO
ALTER TABLE [dbo].[POI] CHECK CONSTRAINT [FK_POI_StreetCrossing]
GO
ALTER TABLE [dbo].[Route]  WITH CHECK ADD  CONSTRAINT [FK_Route_Type] FOREIGN KEY([TypeID])
REFERENCES [dbo].[Type] ([ID])
GO
ALTER TABLE [dbo].[Route] CHECK CONSTRAINT [FK_Route_Type]
GO
ALTER TABLE [dbo].[RoutePiece]  WITH CHECK ADD  CONSTRAINT [FK_RoutePiece_Route] FOREIGN KEY([RouteID])
REFERENCES [dbo].[Route] ([ID])
GO
ALTER TABLE [dbo].[RoutePiece] CHECK CONSTRAINT [FK_RoutePiece_Route]
GO
ALTER TABLE [dbo].[RoutePiece]  WITH CHECK ADD  CONSTRAINT [FK_RoutePiece_StreetCrossing] FOREIGN KEY([StreetCrossingID])
REFERENCES [dbo].[StreetCrossing] ([ID])
GO
ALTER TABLE [dbo].[RoutePiece] CHECK CONSTRAINT [FK_RoutePiece_StreetCrossing]
GO
ALTER TABLE [dbo].[Street]  WITH CHECK ADD  CONSTRAINT [FK_Street_Colonia] FOREIGN KEY([ColoniaID])
REFERENCES [dbo].[Colonia] ([ID])
GO
ALTER TABLE [dbo].[Street] CHECK CONSTRAINT [FK_Street_Colonia]
GO
ALTER TABLE [dbo].[StreetCrossing]  WITH CHECK ADD  CONSTRAINT [FK_StreetCrossing_Street] FOREIGN KEY([StreetID])
REFERENCES [dbo].[Street] ([ID])
GO
ALTER TABLE [dbo].[StreetCrossing] CHECK CONSTRAINT [FK_StreetCrossing_Street]
GO
ALTER TABLE [dbo].[StreetCrossing]  WITH CHECK ADD  CONSTRAINT [FK_StreetCrossing_Street1] FOREIGN KEY([Street2ID])
REFERENCES [dbo].[Street] ([ID])
GO
ALTER TABLE [dbo].[StreetCrossing] CHECK CONSTRAINT [FK_StreetCrossing_Street1]
GO


INSERT [dbo].[Type] ([ID], [Name], [AverageSpeed], [HasNamedStationList], [ShowInWeb], [PriceTypeID], [PriceDefinition], [Frequency], [IsWalkingType], [SeoName]) VALUES (1, N'Bus', 21.5, 0, 1, 2, N'5.5;15|12.5', 10, 0, N'bus')
GO
INSERT [dbo].[Type] ([ID], [Name], [AverageSpeed], [HasNamedStationList], [ShowInWeb], [PriceTypeID], [PriceDefinition], [Frequency], [IsWalkingType], [SeoName]) VALUES (2, N'Walking', 5, 0, 0, 4, NULL, 10, 1, N'walking')
GO