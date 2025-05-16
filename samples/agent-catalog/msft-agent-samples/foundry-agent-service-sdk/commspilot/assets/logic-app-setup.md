# Logic app flow to send emails through Outlook

### Here are the steps to create a new Logic Apps workflow for function calling.

1. In the Azure portal search box, enter logic apps, and select Logic apps.
2. On the Logic apps page toolbar, select Add.
3. On the Create Logic App page, first select the Plan type for your logic app resource. That way, only the options for that plan type appear.
4. In the Plan section, for the Plan type, select Consumption to view only the consumption logic app resource settings.
5. Provide the following information for your logic app resource: Subscription, Resource Group, Logic App name, and Region.
6. When you're ready, select Review + Create.
7. On the validation page that appears, confirm all the provided information, and select Create.
8. After Azure successfully deploys your logic app resource, select Go to resource. Or, find and select your logic app resource by typing the name in the Azure search box.
9. Open the Logic Apps workflow in designer. Select Development Tools + Logic app designer. This opens your empty workflow in designer. Or you select Blank Logic App from templates
10. Now you're ready to add one more step in the workflow. A workflow always starts with a single trigger, which specifies the condition to meet before running any subsequent actions in the workflow.
11. Your workflow is required to have a Request trigger to generate a REST endpoint, and a response action to return the response to Azure AI Foundry when this workflow is invoked.
12. Add a trigger (Request)
Select Add a trigger and then search for request trigger. Select the When an HTTP request is received operation.

![image](https://github.com/user-attachments/assets/0221d407-e7c8-477a-8fcd-316fbaa5f1ca)

Provide the JSON schema for the request. If you do not have the schema use the option to generate schema

![image](https://github.com/user-attachments/assets/5593f822-d0b0-4690-ba98-613d75608478)

The schema used in this sample:

```
{
  "type": "object",
  "properties": {
    "email_to": {
      "type": "string"
    },
    "email_subject": {
      "type": "string"
    },
    "email_body": {
      "type": "string"
    }
  }
}
```
Save the workflow. This will generate the REST endpoint for the workflow.

Now, create the next action in the flow - Send an email similar to the following example:
![image](https://github.com/user-attachments/assets/6de2e733-a647-4406-a549-a77f92855bf8)

Alternatively, use the pre-built "Send email using Outlook" sample in AI Foundry:
![image](https://github.com/user-attachments/assets/1995414e-ebbe-4925-8980-e845ed81a172)



