﻿## 项目说明  
1. 环境  
.net 5 控制台 / vs2019  / sqlserver2012+  / mariadb 10.5

2. 主要逻辑  
读取数据库表信息,生成代码类和页面文件.使用razorengine.netcore库实现代码生成.  
<https://github.com/Antaris/RazorEngine>

3. 用途  
用于典型的CRUD项目,自动生成接口和页面代码.

#### 文件命名  
1. 程序要求输入一个,数据表名字(以下简称'表名'),和程序项目命名空间名(以下简称 'ns')  

2. 输出目录  
输出根目录: 默认程序运行目录下,"CreateCode" 目录.以表名建立子目录:  
```
 CreateCode/表名/files
```

|文件类型|输出文件名|命名空间|
|----|----|----|
|实体类|表名M.cs|ns.Entity|
|Dal类|表名Dal.cs|ns.DAL|
|Bll类|表名Bll.cs|ns.BLL|
|Api类|表名Api.cs|ns|
||||
|列表页|表名list.cshtml||
|编辑页|表名edit.cshtml||
||||
|表文档|表名.doc.html||


#### 模板
1.模板目录 '/tplcshtml'  

|模板|名字|说明|
|----|----|----|
|api接口|ApiCore.cshtml|.net core版本|
|api接口|Api.cshtml|IHttpHandler版本|
|bll|Bll.cshtml|业务逻辑处理类|
|dal|Dal.cshtml|存取数据库|
|实体类|Entity.cshtml|对应数据表类|
||||
|列表页面|List.cshtml||
|编辑页面|Edit.cshtml||
||||
|文档|TableDoc.cshtml|数据表说明文档|

#### 模板定义约定
1. list模板,显示表格数据的div容器,id取名
```
    <div id="表名list"></div>
```
#### 建表约定
1. 对于sqlserver使用smms建表,mariadb使用heidisql建表.尽量使用5种常用字段类型.sqlserver有中文使用nvarchar,否则使用varchar.整数用int/long,小数用decimal

2. 主键取名 id

3. 填写字段注释时,先写字段名字,这个名字用于页面表格的标题.然后空一格,程序用这个空格判断.  
例如: "订单时间 订单生成时间,数据库默认当前时间."  
两句话间有一个空格,前面是字段名字,后面是解释.  
"订单时间" ,这个会用于新增/编辑页面表单的标题.

#### globlejs目录
1. 项目的全局js库,包含接口约定.具体项目具体实现.
2. 目录下所有文件将合并成一个文件,globle.js, _start, _end 这两个文件是开头部分和结尾部分.合并配置文件是bundleconfig.json.