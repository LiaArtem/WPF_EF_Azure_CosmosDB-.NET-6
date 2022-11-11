# WPF_EF_Azure_CosmosDB-.NET-7
WPF project - Test project using Entity Framework Core to the Azure Cosmos database.

Создаем базу данных Azure Cosmos DB на портале:
Database name: dbserver-test-lia

Подключение:
Входим на портале в базу данных dbserver-test-lia, и слева ищем Настройки - Ключи - берем URI и Первичный ключ
Прописываем в файле App.config
 - EndpointUri value - https://dbserver-test-lia.documents.azure.com:443/
 - PrimaryKey  value - Первичный ключ