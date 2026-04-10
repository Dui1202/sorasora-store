import { Card } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

const ProductDescriptionSection = () => {
  const tabs = [
    {
      value: "description",
      content: "Description",
    },
    {
      value: "reviews",
      content: "Reviews",
    },
    {
      value: "faq",
      content: "Faq",
    },
  ];
  return (
    <>
      <Card className="px-5">
        <Tabs className="" defaultValue={tabs[0].value}>
          <TabsList variant="line">
            {tabs.map((tab) => (
              <TabsTrigger
                key={tab.value}
                value={tab.value}
                className="text-xl"
              >
                {tab.content}
              </TabsTrigger>
            ))}
          </TabsList>
          {tabs.map((tab) => (
            <TabsContent key={tab.value} value={tab.value}>
              {tab.content}
            </TabsContent>
          ))}
        </Tabs>
      </Card>
    </>
  );
};

export default ProductDescriptionSection;
