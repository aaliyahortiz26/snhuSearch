import requests
from bs4 import BeautifulSoup

URL='https://realpython.github.io/fake-jobs/'
page = requests.get(URL)

print(page.text) # This scrapes the code, but how it's just HTML. It does not handle javascript only responses

soup = BeautifulSoup(page.content, "html.parser")

results = soup.find(id="ResultsContainer")

print(results.prettify())

job_elements = results.find_all("div", class_="card-content")
for job_element in job_elements:
    print(job_element, end="\n"*2)

print("Pre stripping text")
for job_element in job_elements:
    title_element = job_element.find("h2", class_="title")
    company_element = job_element.find("h3", class_="company")
    location_element = job_element.find("p", class_="location")
    print(title_element)
    print(company_element)
    print(location_element)
    print()


print("Stripping") # Strips the html stuff away on the finds and prints the text
for job_element in job_elements:
    title_element = job_element.find("h2", class_="title")
    company_element = job_element.find("h3", class_="company")
    location_element = job_element.find("p", class_="location")
    print(title_element.text.strip())
    print(company_element.text.strip())
    print(location_element.text.strip())
    print()

python_jobs = results.find_all("h2", string="Python") # Finds all <h2> elements containing string matches "Python"
print(python_jobs) # There was a python job result but it does not show. "string=" does EXACT matches

python_jobs = results.find_all( # Looks at text for <h2>, converts to lower case, and checks for string
    "h2", string=lambda text: "python" in text.lower()
)
print(python_jobs) # Prints out raw, but doing python_jobs.text.strip() won't work. The text we want is buried so we must use the following lines of code:

python_job_elements = [
    h2_element.parent.parent.parent for h2_element in python_jobs
]

for job_element in python_job_elements:
    # -- snip --
    links = job_element.find_all("a")
    for link in links:
        print(link.text.strip())

for job_element in python_job_elements:
    # -- snip --
    links = job_element.find_all("a")
    for link in links:
        link_url = link["href"]
        print(f"Apply here: {link_url}\n")

print("Testing my own website down below")
print("------------------------------------------------------------------------------")
print()

#URL = 'https://www.payscale.com/research/US/Job=Software_Engineer/Salary'
#page = requests.get(URL)

#print(page.text)
#soup = BeautifulSoup(page.content, "html.parser")
#results = soup.find(id="ResultsContainer")
#print(results.prettify())
# Breaks, let's try the javascript version on dynamic websites
path = 'C:\\Users\\Jesse\\Downloads\\geckodriver-v0.30.0-win64'
from selenium import webdriver
#from selenium.webdriver.chrome.options import Options
from selenium.webdriver.firefox.options import Options as FirefoxOptions
import pandas as pd

def connectFirefox():
    options = FirefoxOptions()
    options.add_argument("--headless")
    driver = webdriver.Firefox()
    print("Firefox Headless Browser Invoked")
    return driver

#driver = connectChrome()
driver = connectFirefox()
driver.get("https://www.nintendo.com/")

h1 = driver.find_element_by_name('h1')
h1 = driver.find_element_by_class_name('someclass')
h1 = driver.find_element_by_xpath('//h1')
h1 = driver.find_element_by_id('greatID')
all_links = driver.find_elements_by_tag_name('a')






print("Headless Browser closing")
driver.quit()


#driver = webdriver.Firefox()
#driver.get('https://www.lazada.sg/#')

#options = Options()
#options.headless = True
#options.add_argument("--window-size=1920,1200")

#driver = webdriver.Chrome(options=options, executable_path=DRIVER_PATH)
#driver = webdriver.Firefox(options=options, executable_path=path)
#driver.get("https://www.nintendo.com/")
#print(driver.page_source)
#driver.quit()





