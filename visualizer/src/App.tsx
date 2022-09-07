import { useLayoutEffect, useState } from "react";
import { Grid } from "./components/Grid/Grid";
import { Playground } from "./components/Playground/Playground";

function App() {
    const [page, setPage] = useState('playground');
    useLayoutEffect(() => {
        if (document.location.pathname === '/grid') {
            setPage('grid');
        }
    }, []);

  return (
    <>
      {page === 'playground' && <Playground />}
      {page === 'grid' && <Grid />}
    </>
  );
}

export default App;
