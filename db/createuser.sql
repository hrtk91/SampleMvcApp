CREATE DATABASE SampleMVCAppContext
go

CREATE LOGIN TEST
WITH
  PASSWORD = 'Abcd1234#',
  DEFAULT_DATABASE = SampleMVCAppContext,
  CHECK_EXPIRATION = OFF, -- 有効期限チェックしない
  CHECK_POLICY = OFF -- パスワードの複雑性要件をチェックしない
go

use SampleMVCAppContext
go

CREATE USER TEST
go

EXEC sp_addrolemember 'db_owner', 'TEST'
go
