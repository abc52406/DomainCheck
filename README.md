# 可用域名检查工具
<br/> 想要注册一个好一点的域名，上网翻来覆去都发现被人注册掉了，这样一个一个找下去，得找到什么时候？
<br/>
<br/> 用这个工具可以对剩下的域名进行地毯式搜索，然后你就能从中选择自己想要的。
<br/> 
<br/> 此工具支持 的检索规则包括
- 检索指定后缀的域名（如.com .cn .com.cn）
- 检索指定长度的数字域名
- 检索指定长度的字母域名
- 检索指定字数的汉语拼音域名
- 对于以上三种检索方式，增加固定前缀或后缀进行检索
<br/>
<br/>举例说明
<br/>![example](http://pengbo.info/examples/example-domaincheck.png)
<br/>上述配置方式，会查找
- .com .cn .com.cn域名
- 长度不超过4的前缀为test的所有数字域名，如test1.com test11.com test111.com test1111.com
- 长度不超过4的前缀为test的所有字母域名，如testa.com testaa.com testaaa.com testaaaa.com
- 字数不超过1的前缀为test的所有拼音域名，如testzhang.com testchun.com testyun.com testhao.com
- 由于域名查询接口不允许频繁调用，所以域名给定查询间隔为100ms，如果在一台同时运行两个程序进行查找，则需要把时间间隔翻倍
- 选择使用数据库，则会把所有的查找记录存储在根目录下的access文件中（确保电脑安装了office软件），系统查询域名是否可用，会先查询数据库，再调用接口查询，提高查询速度（但是没法保证数据的实时性 ）
- 如果查询范围过大，耗时太长，担心断电等意外因素导致前功尽弃，可以选择使用数据库
