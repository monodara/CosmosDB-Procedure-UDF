# Azure Cosmos DB for NoSQL: Stored Procedures, UDFs, and Triggers

## Overview

This document provides an overview of using JavaScript methods and syntax for creating and managing stored procedures, user-defined functions (UDFs), and triggers in Azure Cosmos DB for NoSQL.

## Methods and Objects Reference

### Context

- **`getContext()`**: Retrieves the context object for the current request.
  - **Usage**: Provides access to the request and response objects as well as the collection.
  - **Example**:
    ```javascript
    var context = getContext()
    ```

### Request

- **`getRequest()`**: Retrieves the request object for the current operation.

  - **Usage**: Used in pre-triggers to access the details of the request being processed.
  - **Example**:
    ```javascript
    var request = context.getRequest()
    ```

- **`getBody()`**: Retrieves the body of the request.
  - **Usage**: Used to get the document being inserted or updated.
  - **Example**:
    ```javascript
    var document = request.getBody()
    ```

### Response

- **`getResponse()`**: Retrieves the response object for the current operation.

  - **Usage**: Used to set the body of the response or to access the result of the operation.
  - **Example**:
    ```javascript
    var response = context.getResponse()
    ```

- **`setBody(body)`**: Sets the body of the response.
  - **Usage**: Used to define the output of a stored procedure or trigger.
  - **Example**:
    ```javascript
    response.setBody(documentCreated)
    ```

### Collection

- **`getCollection()`**: Retrieves the collection object representing the current collection.

  - **Usage**: Used to perform operations like creating, reading, updating, and deleting documents within the collection.
  - **Example**:
    ```javascript
    var container = context.getCollection()
    ```

- **`getSelfLink()`**: Retrieves the self-link of the collection.

  - **Usage**: Used to get the unique URI of the collection.
  - **Example**:
    ```javascript
    var selfLink = container.getSelfLink()
    ```

- **`createDocument(collectionLink, document, options, callback)`**: Creates a document in the collection.

  - **Usage**: Adds a new document to the collection.
  - **Example**:
    ```javascript
    container.createDocument(
      container.getSelfLink(),
      doc,
      function (err, documentCreated) {
        if (err) throw new Error("Error" + err.message)
        response.setBody(documentCreated)
      }
    )
    ```

- **`readDocument(documentLink, options, callback)`**: Reads a document from the collection.

  - **Usage**: Retrieves a document based on its link.
  - **Example**:
    ```javascript
    container.readDocument(
      documentLink,
      options,
      function (err, document, responseOptions) {
        if (err) throw new Error("Error" + err.message)
        response.setBody(document)
      }
    )
    ```

- **`replaceDocument(documentLink, document, options, callback)`**: Replaces an existing document.

  - **Usage**: Updates an existing document in the collection.
  - **Example**:
    ```javascript
    container.replaceDocument(
      documentLink,
      document,
      function (err, documentUpdated) {
        if (err) throw new Error("Error" + err.message)
        response.setBody(documentUpdated)
      }
    )
    ```

- **`deleteDocument(documentLink, options, callback)`**: Deletes a document from the collection.

  - **Usage**: Removes a document from the collection.
  - **Example**:
    ```javascript
    container.deleteDocument(
      documentLink,
      options,
      function (err, responseOptions) {
        if (err) throw new Error("Error" + err.message)
        response.setBody(responseOptions)
      }
    )
    ```

- **`queryDocuments(collectionLink, query, options, callback)`**: Queries documents in the collection.
  - **Usage**: Executes a query against the documents in the collection.
  - Paramaters:
    - collectionLink (string): The self-link of the collection to query.
    - query (object or string): The SQL-like query or query object.
    - options (object, optional): Optional settings for the query execution.
    - callback (function): The callback function to handle the query result.
      - err (object): An error object if the query fails, otherwise null.
      - documents (array): An array of documents returned by the query.
      - responseOptions (object): Additional response options.
  - **Example**:
    ```javascript
    var query = "SELECT * FROM c WHERE c.id = @id"
    var parameters = [{ name: "@id", value: "some-id" }]
    container.queryDocuments(
      container.getSelfLink(),
      { query: query, parameters: parameters },
      options,
      function (err, documents, responseOptions) {
        if (err) throw new Error("Error" + err.message)
        response.setBody(documents)
      }
    )
    ```

### Error Handling

- **`throw new Error(message)`**: Throws an error with a specific message.
  - **Usage**: Used to handle errors and stop execution.
  - **Example**:
    ```javascript
    throw new Error('Document must contain a "name" property.')
    ```

## JavaScript Query API with `__` Alias

The double-underscore (`__`) is an alias for `getContext().getCollection()` when using the JavaScript query API.

### SQL to JavaScript Query API Mappings

**SQL SECLECT**:

```sql
SELECT * FROM docs
```

JavaScript:

```javascript
__.map(function (doc) {
  return doc
})
```

**SQL Projection:**

```sql
SELECT docs.id, docs.message AS msg, docs.actions FROM docs
```

JavaScript:

```javascript
__.map(function (doc) {
  return {
    id: doc.id,
    msg: doc.message,
    actions: doc.actions,
  }
})
```

**_SQL Filtering:_**

```sql
SELECT * FROM docs WHERE docs.id = "X998_Y998"
```

JavaScript:

```javascript
__.filter(function (doc) {
  return doc.id === "X998_Y998"
})
```

**SQL Array Contains:**

```sql
SELECT \* FROM docs WHERE ARRAY_CONTAINS(docs.Tags, 123)
```

JavaScript:

```javascript
\_\_.filter(function(x) {
return x.Tags && x.Tags.indexOf(123) > -1;
});
```

**SQL Chaining Filters and Projections:**

```sql
SELECT docs.id, docs.message AS msg FROM docs WHERE docs.id = "X998_Y998"
```

JavaScript:

```javascript
__.chain()
  .filter(function (doc) {
    return doc.id === "X998_Y998"
  })
  .map(function (doc) {
    return {
      id: doc.id,
      msg: doc.message,
    }
  })
  .value()
```

**SQL Join and Order By:**

```sql
SELECT VALUE tag FROM docs JOIN tag IN docs.Tags ORDER BY docs.\_ts
```

JavaScript:

```javascript
__.chain()
.filter(function(doc) {
return doc.Tags && Array.isArray(doc.Tags);
})
.sortBy(function(doc) {
return doc.\_ts;
})
.pluck("Tags")
.flatten()
.value();
```

## Supported JavaScript Functions

- `chain() ... .value([callback] [, options])`: Starts a chained call that must be terminated with value().
- `filter(predicateFunction [, options] [, callback])`: Filters the input using a predicate function that returns true/false to filter in/out input documents into the resulting set. Similar to a WHERE clause in SQL.
- `flatten([isShallow] [, options] [, callback])`: Combines and flattens arrays from each input item into a single array. Similar to SelectMany in LINQ.
- `map(transformationFunction [, options] [, callback])`: Applies a projection using a transformation function that maps each input item to a JavaScript object or value. Similar to a SELECT clause in SQL.
- `pluck([propertyName] [, options] [, callback])`: Shortcut for a map that extracts the value of a single property from each input item.
- `sortBy([predicate] [, options] [, callback])`: Produces a new set of documents by sorting the documents in the input document stream in ascending order using the given predicate. Similar to an ORDER BY clause in SQL.
- `sortByDescending([predicate] [, options] [, callback])`: Produces a new set of documents by sorting the documents in the input document stream in descending order using the given predicate. Similar to an ORDER BY x DESC clause in SQL.
- `unwind(collectionSelector, [resultSelector], [options], [callback])`: Performs a self-join with an inner array and adds results from both sides as tuples to the result projection. Similar to SelectMany in .NET LINQ.

## Document Object (`doc`)

The `doc` object represents a document in the Azure Cosmos DB collection. It is typically used within stored procedures, UDFs, and triggers to manipulate the data stored in the collection.

### Common Properties

- **`id`**: The unique identifier for the document.
  - **Example**:
    ```javascript
    var documentId = doc.id
    ```
- **`_self`**: The unique addressable URI of the document.
  - **Example**:
    ```javascript
    var documentLink = doc._self
    ```
- **`_ts`**: The timestamp of the document, indicating when it was last modified.
  - **Example**:
    ```javascript
    var lastModified = doc._ts
    ```
- **`_etag`**: The entity tag for the document used for concurrency control.
  - **Example**:
    ```javascript
    var etag = doc._etag
    ```
- **`[custom properties]`**: Any custom properties defined in your documents, such as `name`, `address`, `tags`, etc.
  - **Example**:
    ```javascript
    var name = doc.name
    var address = doc.address
    var tags = doc.tags
    ```
