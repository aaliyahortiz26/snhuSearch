import requests
from bs4 import BeautifulSoup
from lxml import html
from selenium.webdriver import Firefox
from selenium import webdriver
from webdriver_manager.firefox import GeckoDriverManager
from selenium.webdriver.common.by import By

url = 'https://www.mysql.com/'
response = requests.get(url)
soup = BeautifulSoup(response.text, 'html.parser')
print(soup.title)

blog_titles = soup.select('h2.blog-card__content-title')
for title in blog_titles:
    print(title.text)

tree = html.fromstring(response.text)

# //h2[@class = "blog-card__content-title"]/text()

# driver = Firefox(executable_path='/usr/bin/firefox')

driver = webdriver.Firefox(executable_path = GeckoDriverManager().install())

driver.get(url)
print("Printed body below")
var = driver.find_elements(By.XPATH, "/html/body")


f = open("webscrape.txt", "a", encoding="utf8")
for element in var:
    print(element.text)
    f.write(element.text)

driver.close()
f.close()