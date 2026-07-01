# 10Authentication

这是一个用于学习 ASP.NET Core 自定义认证的 .NET 10 Web 项目。

项目实现了一个完全自定义的认证方案：

```http
Authorization: Demo <token>
```

## 核心流程

```text
POST /auth/login
  -> 校验用户名和密码
  -> 签发自定义 Demo Token

GET /secure/profile
  -> DemoTokenAuthenticationHandler 读取 Authorization Header
  -> 校验 Token 格式
  -> 校验 HMAC SHA256 签名
  -> 校验 issuer / audience
  -> 校验 iat / exp
  -> 校验 token 是否已撤销
  -> 校验用户是否存在、是否启用
  -> 校验 security_stamp
  -> 构建 ClaimsPrincipal
  -> 进入授权策略
```

## 示例账号

也可以访问：

```http
GET /auth/demo-accounts
```

内置账号：

| 用户名 | 密码 | 角色 | 权限 |
| --- | --- | --- | --- |
| admin | admin123! | Admin, User | profile.read, admin.read, payroll.read, token.revoke |
| alice | alice123! | User | profile.read |
| disabled | disabled123! | User | profile.read |

## 运行

```powershell
dotnet run --project .\10Authentication\10Authentication.csproj
```

默认 HTTP 地址：

```text
http://localhost:5065
```

## 登录

```powershell
$login = Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5065/auth/login" `
  -ContentType "application/json" `
  -Body '{"userName":"admin","password":"admin123!"}'

$token = $login.accessToken
```

## 访问受保护接口

```powershell
Invoke-RestMethod `
  -Uri "http://localhost:5065/secure/profile" `
  -Headers @{ Authorization = "Demo $token" }
```

## 测试授权策略

Admin 角色：

```powershell
Invoke-RestMethod `
  -Uri "http://localhost:5065/secure/admin" `
  -Headers @{ Authorization = "Demo $token" }
```

权限策略：

```powershell
Invoke-RestMethod `
  -Uri "http://localhost:5065/secure/payroll" `
  -Headers @{ Authorization = "Demo $token" }
```

撤销 Token：

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5065/auth/logout" `
  -Headers @{ Authorization = "Demo $token" }
```

撤销后再次访问受保护接口，会返回 401。

## 目录说明

| 目录 | 说明 |
| --- | --- |
| `Authentication` | 自定义认证 Scheme、Handler、Claims、DI 注册 |
| `Tokens` | 自定义 Token 签发、签名、校验、撤销存储 |
| `Users` | 演示用户仓储、密码哈希 |
| `Contracts` | 请求和响应 DTO |
| `Endpoints` | 登录、登出、受保护资源接口 |

## 学习重点

- `AddAuthentication().AddScheme<TOptions, THandler>()` 如何注册自定义认证。
- `AuthenticationHandler<TOptions>` 如何读取请求并返回 `AuthenticateResult`。
- `ClaimsPrincipal` 如何由 Token 转换而来。
- `RequireAuthorization()` 如何触发认证和授权。
- 401 Authentication 和 403 Authorization 的区别。
- 自定义 Token 为什么必须校验签名、过期时间、用户状态和撤销状态。

## 注意

这个项目为了学习自定义认证，故意没有直接使用 JWT Bearer。真实生产系统更推荐优先使用成熟标准，例如 Cookie Authentication、JWT Bearer、OAuth 2.0、OpenID Connect。
