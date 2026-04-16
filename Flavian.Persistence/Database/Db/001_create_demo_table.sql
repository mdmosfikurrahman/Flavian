IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='demo' AND xtype='U')
CREATE TABLE [dbo].[demo] (
    [id]            UNIQUEIDENTIFIER    NOT NULL,
    [row_id]        BIGINT              IDENTITY(1,1) NOT NULL,
    [name]          NVARCHAR(100)       NOT NULL,
    [description]   NVARCHAR(255)       NULL,
    [is_active]     BIT                 NOT NULL DEFAULT 1,
    [created_by]    UNIQUEIDENTIFIER    NOT NULL,
    [business_unit] INT                 NOT NULL,
    [created_date]  DATETIME2           NOT NULL DEFAULT GETDATE(),
    [modified_by]   UNIQUEIDENTIFIER    NULL,
    [modified_date] DATETIME2           NULL,
    [is_deleted]    BIT                 NOT NULL DEFAULT 0,
    CONSTRAINT [PK_demo] PRIMARY KEY ([id])
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='demo_audit' AND xtype='U')
CREATE TABLE [dbo].[demo_audit] (
    [id]              UNIQUEIDENTIFIER    NOT NULL,
    [row_id]          BIGINT              IDENTITY(1,1) NOT NULL,
    [event_id]        UNIQUEIDENTIFIER    NOT NULL,
    [action_name]     NVARCHAR(50)        NOT NULL,
    [action_details]  NVARCHAR(1000)      NOT NULL,
    [created_by]      UNIQUEIDENTIFIER    NOT NULL,
    [created_date]    DATETIME2           NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_demo_audit] PRIMARY KEY ([id]),
    CONSTRAINT [FK_demo_audit_demo] FOREIGN KEY ([event_id]) REFERENCES [dbo].[demo]([id])
);
